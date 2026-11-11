using System.Reflection;
using System.Runtime.CompilerServices;
using Bogus;
using CRM.AlMohtaref.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Seeds every empty table by reading db.Model (tables, columns, FKs, unique indexes, enums, max lengths).
// Add / rename / remove a property or an entity and the next run adapts by itself.
//
// - parents are seeded before children (required FKs)
// - circular required FKs (Person <-> User): the back edge gets a placeholder, the FK check of that one
//   constraint is switched off while inserting, then every placeholder is replaced by a real parent and the
//   constraint is checked again. Everything runs inside the transaction of DataSeeder.
// - Guid keys are generated here, so every key is known before SaveChanges
// - one-to-one FKs (unique) never share a parent: Person rows are used once by User / Contact / Student
// - FK named CreatedBy* / UpdatedBy* / ModifiedBy* always points to the first row of the parent (the admin)
// - any property whose name contains "password" gets the hash of SeedRules.PlainPassword
// - an entity with a CHECK constraint and 2+ optional FKs gets only ONE of them per row (like ContactTag)
internal sealed class AutoSeeder
{
    private sealed class Row : Dictionary<string, object?> { }

    private sealed record Pending(IEntityType Dependent, IForeignKey Fk, int RowIndex, object Placeholder);

    private readonly AppDbContext _db;
    private readonly CancellationToken _ct;
    private readonly Faker _f = new("ar") { Random = new Randomizer(12345) }; // fixed seed: same data every run
    private readonly string _passwordHash = new PasswordHasher().HashPassword(SeedRules.PlainPassword);

    private readonly List<IEntityType> _seedable;
    private readonly HashSet<IForeignKey> _backEdges = new();
    private readonly Dictionary<IEntityType, List<Row>> _rows = new();
    private readonly Dictionary<IEntityType, HashSet<int>> _used = new();
    private readonly Dictionary<IEntityType, int> _rowCache = new();
    private readonly List<Pending> _pending = new();

    public AutoSeeder(AppDbContext db, CancellationToken ct)
    {
        _db = db;
        _ct = ct;
        // db.Model is the read-optimized runtime model (EF Core 9+): it lacks relational metadata such as
        // constraint names and check constraints. The design-time model has everything.
        var model = db.GetService<IDesignTimeModel>().Model;
        _seedable = model.GetEntityTypes().Where(IsSeedable).ToList();
    }

    public async Task RunAsync()
    {
        foreach (var et in Sort(_seedable))
        {
            var existing = await LoadAsync(et);
            if (existing.Count > 0)
            {
                _rows[et] = existing;
                await FixupAsync(et);
                continue;
            }

            var created = await FillAsync(et);
            _rows[et] = created;
            _db.ChangeTracker.Clear();

            Console.WriteLine("[seed] " + et.ClrType.Name + ": " + created.Count + " rows");
            await FixupAsync(et);
        }

        if (_pending.Count > 0)
        {
            var names = string.Join(", ", _pending.Select(p => p.Dependent.ClrType.Name + "." + p.Fk.Properties[0].Name).Distinct());
            throw new InvalidOperationException("Seed: circular foreign keys could not be resolved: " + names);
        }
    }

    // ---------------------------------------------------------------- model

    private static bool IsSeedable(IEntityType et)
    {
        return !et.IsOwned()
               && !et.ClrType.IsAbstract
               && et.BaseType == null
               && !et.HasSharedClrType
               && et.FindPrimaryKey() != null
               && et.GetTableName() != null
               && et.GetViewName() == null;
    }

    // parents first (required FKs). A required FK that closes a circle is stored in _backEdges.
    private List<IEntityType> Sort(List<IEntityType> all)
    {
        var result = new List<IEntityType>();
        var state = new Dictionary<IEntityType, int>();

        void Visit(IEntityType e)
        {
            state[e] = 1;

            foreach (var fk in e.GetForeignKeys())
            {
                var parent = fk.PrincipalEntityType;
                if (parent == e || !fk.IsRequired || !all.Contains(parent)) continue;

                if (state.TryGetValue(parent, out var s))
                {
                    if (s == 1) _backEdges.Add(fk);
                    continue;
                }

                Visit(parent);
            }

            state[e] = 2;
            result.Add(e);
        }

        foreach (var e in all.OrderBy(x => x.Name))
            if (!state.ContainsKey(e))
                Visit(e);

        // Second pass: optional FKs must also see their parents already seeded (ContactTag -> Contact / Student),
        // otherwise both optional FKs stay null. An optional edge is added only when it cannot create a cycle.
        var deps = all.ToDictionary(e => e, _ => new HashSet<IEntityType>());

        foreach (var e in all)
            foreach (var fk in e.GetForeignKeys())
                if (fk.IsRequired && !_backEdges.Contains(fk) && fk.PrincipalEntityType != e && all.Contains(fk.PrincipalEntityType))
                    deps[e].Add(fk.PrincipalEntityType);

        bool DependsOn(IEntityType from, IEntityType target, HashSet<IEntityType> visited)
        {
            if (!visited.Add(from)) return false;
            foreach (var d in deps[from])
                if (d == target || DependsOn(d, target, visited))
                    return true;
            return false;
        }

        foreach (var e in all.OrderBy(x => x.Name))
            foreach (var fk in e.GetForeignKeys())
            {
                var parent = fk.PrincipalEntityType;
                if (fk.IsRequired || parent == e || !all.Contains(parent) || deps[e].Contains(parent)) continue;
                if (!DependsOn(parent, e, new HashSet<IEntityType>()))
                    deps[e].Add(parent);
            }

        var ordered = new List<IEntityType>();
        var emitted = new HashSet<IEntityType>();

        void Emit(IEntityType e)
        {
            if (!emitted.Add(e)) return;
            foreach (var d in deps[e].OrderBy(x => x.Name))
                Emit(d);
            ordered.Add(e);
        }

        foreach (var e in all.OrderBy(x => x.Name))
            Emit(e);

        return ordered;
    }

    private int RowsFor(IEntityType et, HashSet<IEntityType>? stack = null)
    {
        if (_rowCache.TryGetValue(et, out var cached)) return cached;

        var name = et.ClrType.Name;
        var configured = SeedRules.Lookups.TryGetValue(name, out var lookup) ? lookup.Length
            : SeedRules.RowCounts.TryGetValue(name, out var n) ? n
            : SeedRules.DefaultRows;

        stack ??= new HashSet<IEntityType>();
        if (!stack.Add(et)) return configured;

        // a one-to-one child needs its own parent row
        var demand = 0;
        foreach (var child in _seedable)
        {
            if (child == et) continue;
            if (child.GetForeignKeys().Any(k => IsExclusive(k) && !k.IsOwnership && k.PrincipalEntityType == et && !IsAudit(k)))
                demand += RowsFor(child, stack);
        }

        stack.Remove(et);
        return _rowCache[et] = Math.Max(configured, demand);
    }

    private static bool IsAudit(IForeignKey fk)
    {
        return fk.Properties.Any(p =>
        {
            var n = p.Name.ToLowerInvariant();
            return n.StartsWith("createdby") || n.StartsWith("updatedby") || n.StartsWith("modifiedby");
        });
    }

    // one-to-one FKs, and FKs like Contacts.PersonId -> People.PersonId (same name as the parent key):
    // the child "is a" person, so every parent row is used by at most one child row.
    private static bool IsExclusive(IForeignKey fk)
    {
        if (fk.IsUnique) return true;

        return fk.Properties.Count == 1
               && fk.Properties[0].Name != "Id"
               && fk.Properties[0].Name == fk.PrincipalKey.Properties[0].Name;
    }

    private HashSet<int> Used(IEntityType et)
    {
        if (!_used.TryGetValue(et, out var set)) _used[et] = set = new HashSet<int>();
        return set;
    }

    // row = index of the child row. Normal FKs go round-robin over the parent rows, so every parent
    // is used (every user gets a permission, every track a campaign...) before any parent repeats.
    private Row Pick(IForeignKey fk, List<Row> parents, int row = 0)
    {
        if (IsAudit(fk)) return parents[0];

        if (IsExclusive(fk))
        {
            var used = Used(fk.PrincipalEntityType);
            for (var k = 0; k < parents.Count; k++)
                if (used.Add(k)) return parents[k];

            throw new InvalidOperationException("Seed: not enough " + fk.PrincipalEntityType.ClrType.Name + " rows for a one-to-one relation.");
        }

        return parents[row % parents.Count];
    }

    // ---------------------------------------------------------------- load existing

    private async Task<List<Row>> LoadAsync(IEntityType et)
    {
        var method = typeof(AutoSeeder)
            .GetMethod(nameof(LoadTyped), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(et.ClrType);

        return await (Task<List<Row>>)method.Invoke(this, null)!;
    }

    private async Task<List<Row>> LoadTyped<T>() where T : class
    {
        var items = await _db.Set<T>().IgnoreQueryFilters().ToListAsync(_ct);
        var rows = items.Select(e => Snapshot(_db.Entry(e))).ToList();
        _db.ChangeTracker.Clear();
        return rows;
    }

    private static Row Snapshot(EntityEntry entry)
    {
        var row = new Row();
        foreach (var p in entry.Properties)
            row[p.Metadata.Name] = p.CurrentValue;
        return row;
    }

    // ---------------------------------------------------------------- fill

    private async Task<List<Row>> FillAsync(IEntityType et)
    {
        var rows = RowsFor(et);
        var pk = et.FindPrimaryKey()!;

        var fks = et.GetForeignKeys()
            .Where(k => !k.IsOwnership && k.PrincipalEntityType != et && _seedable.Contains(k.PrincipalEntityType))
            .ToList();

        var normal = new List<IForeignKey>();
        var placeholders = new List<IForeignKey>();

        foreach (var fk in fks)
        {
            if (_backEdges.Contains(fk))
            {
                if (fk.Properties.Count != 1)
                    throw new NotSupportedException("Seed: circular composite FK on " + et.ClrType.Name + " is not supported.");

                placeholders.Add(fk);
                continue;
            }

            if (_rows.TryGetValue(fk.PrincipalEntityType, out var parents) && parents.Count > 0)
            {
                normal.Add(fk);
                if (IsExclusive(fk) && !IsAudit(fk))
                    rows = Math.Min(rows, parents.Count - Used(fk.PrincipalEntityType).Count);
            }
            else if (fk.IsRequired)
            {
                Console.WriteLine("[seed] " + et.ClrType.Name + ": skipped (required parent " + fk.PrincipalEntityType.ClrType.Name + " has no rows)");
                return new List<Row>();
            }
        }

        // entity with a CHECK constraint and several optional FKs (ContactTag): one FK per row
        var optional = normal.Where(k => !k.IsRequired && !IsAudit(k)).ToList();
        var exclusive = optional.Count > 1 && await HasCheckOnAsync(et, optional);
        if (exclusive)
            Console.WriteLine("[seed] " + et.ClrType.Name + ": one of (" + string.Join(", ", optional.Select(o => o.Properties[0].Name)) + ") per row (CHECK constraint)");

        var table = Table(et);
        foreach (var fk in placeholders)
            await _db.Database.ExecuteSqlRawAsync("ALTER TABLE " + table + " NOCHECK CONSTRAINT [" + fk.GetConstraintName() + "]", _ct);

        var entries = new List<EntityEntry>();
        var newPending = new List<Pending>();
        var seen = new HashSet<string>();
        var fkProperties = et.GetForeignKeys().SelectMany(k => k.Properties).ToHashSet();

        // composite unique indexes (UserPermissions: UserId + RoleId + DepartmentId): never repeat a combination
        var uniqueIndexes = et.GetIndexes().Where(ix => ix.IsUnique && ix.Properties.Count > 1).ToList();
        var seenIndexes = uniqueIndexes.ToDictionary(ix => ix, _ => new HashSet<string>());

        for (var i = 0; i < rows; i++)
        {
            var values = new Dictionary<IProperty, object?>();
            var rowPlaceholders = new List<(IForeignKey Fk, object Value)>();

            var account = et.ClrType.Name == "User"
                ? (i == 0 ? SeedRules.AdminAccount : "user" + i.ToString("00"))
                : et.ClrType.Name.ToLowerInvariant() + (i + 1);

            foreach (var fk in normal)
            {
                if (exclusive && optional.Contains(fk) && optional[i % optional.Count] != fk) continue;

                // SeedRules.FirstRow: fixed parent (by its Name) for row 0, e.g. the admin permission
                Row? parent = null;
                if (i == 0 && SeedRules.FirstRow.TryGetValue(et.ClrType.Name + "." + fk.Properties[0].Name, out var wanted))
                    parent = _rows[fk.PrincipalEntityType].FirstOrDefault(r => r.TryGetValue("Name", out var nm) && nm as string == wanted);

                parent ??= Pick(fk, _rows[fk.PrincipalEntityType], i);
                for (var j = 0; j < fk.Properties.Count; j++)
                    values[fk.Properties[j]] = parent[fk.PrincipalKey.Properties[j].Name];
            }

            foreach (var fk in placeholders)
            {
                var ph = Placeholder(fk.Properties[0], i);
                values[fk.Properties[0]] = ph;
                rowPlaceholders.Add((fk, ph));
            }

            foreach (var p in et.GetProperties())
            {
                if (values.ContainsKey(p) || p.IsShadowProperty()) continue;

                // an FK column that got no value (optional, or skipped on purpose) stays NULL:
                // it must never be filled with a random value
                if (fkProperties.Contains(p)) continue;

                var unique = et.GetIndexes().Any(ix => ix.IsUnique && ix.Properties.Count == 1 && ix.Properties[0] == p);

                if (p.IsPrimaryKey())
                {
                    if (p.ClrType == typeof(Guid)) values[p] = StableGuid(et.ClrType.Name, i);
                    else if (p.ValueGenerated == ValueGenerated.Never) values[p] = MakeValue(et, p, i, account, true);
                    continue;
                }

                if (p.ValueGenerated != ValueGenerated.Never) continue; // identity, computed, default in DB, rowversion

                var v = MakeValue(et, p, i, account, unique);
                if (v != null) values[p] = v;
            }

            // join tables with a composite key: skip duplicates
            if (pk.Properties.Count > 1)
            {
                var key = string.Join("|", pk.Properties.Select(x => values.TryGetValue(x, out var kv) ? kv : null));
                if (!seen.Add(key)) continue;
            }

            var duplicate = false;
            foreach (var ix in uniqueIndexes)
            {
                if (!ix.Properties.All(values.ContainsKey)) continue;

                var ixKey = string.Join("|", ix.Properties.Select(x => values[x]));
                if (!seenIndexes[ix].Add(ixKey)) { duplicate = true; break; }
            }
            if (duplicate) continue;

            var entity = Create(et.ClrType);
            var entry = _db.Add(entity);

            foreach (var (p, v) in values)
                entry.Property(p.Name).CurrentValue = v;

            foreach (var (fk, ph) in rowPlaceholders)
                newPending.Add(new Pending(et, fk, entries.Count, ph));

            entries.Add(entry);
        }

        if (entries.Count == 0) return new List<Row>();

        await _db.SaveChangesAsync(_ct);

        var result = entries.Select(Snapshot).ToList();
        _pending.AddRange(newPending);
        return result;
    }

    // True when a CHECK constraint of the table mentions two or more of the optional FK columns
    // (e.g. "exactly one of StudentId / ContactId"). Read from the real database (sys.check_constraints),
    // because a constraint added with raw SQL in a migration is not part of the EF model.
    private async Task<bool> HasCheckOnAsync(IEntityType et, List<IForeignKey> optional)
    {
        if (et.GetCheckConstraints().Any()) return true;

        var id = StoreObjectIdentifier.Table(et.GetTableName()!, et.GetSchema());
        var columns = optional.Select(fk => fk.Properties[0].GetColumnName(id)).ToList();

        var definitions = await _db.Database
            .SqlQueryRaw<string>("SELECT [definition] AS [Value] FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID({0})", Table(et))
            .ToListAsync(_ct);

        return definitions.Any(d => columns.Count(c => d.Contains("[" + c + "]", StringComparison.OrdinalIgnoreCase)) > 1);
    }

    // replace every placeholder by a real parent as soon as the parent table has rows
    private async Task FixupAsync(IEntityType principal)
    {
        if (!_rows.TryGetValue(principal, out var parents) || parents.Count == 0) return;

        foreach (var group in _pending.Where(x => x.Fk.PrincipalEntityType == principal).GroupBy(x => x.Fk).ToList())
        {
            var fk = group.Key;
            var dependent = group.First().Dependent;
            var prop = fk.Properties[0];
            var table = Table(dependent);
            var column = prop.GetColumnName(StoreObjectIdentifier.Table(dependent.GetTableName()!, dependent.GetSchema()));

            foreach (var item in group)
            {
                var parent = Pick(fk, parents);
                var value = parent[fk.PrincipalKey.Properties[0].Name]!;

                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE " + table + " SET [" + column + "] = {0} WHERE [" + column + "] = {1}",
                    new object[] { value, item.Placeholder }, _ct);

                _rows[dependent][item.RowIndex][prop.Name] = value;
            }

            await _db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE " + table + " WITH CHECK CHECK CONSTRAINT [" + fk.GetConstraintName() + "]", _ct);

            _pending.RemoveAll(x => x.Fk == fk);
        }
    }

    private static string Table(IEntityType et)
    {
        return "[" + (et.GetSchema() ?? "dbo") + "].[" + et.GetTableName() + "]";
    }

    private static object Create(Type type)
    {
        try
        {
            return Activator.CreateInstance(type, nonPublic: true)!;
        }
        catch (Exception)
        {
            return RuntimeHelpers.GetUninitializedObject(type);
        }
    }

    private static readonly DateTime RefDate = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    // The same entity + row index always gives the same Guid, so after the database is deleted and
    // re-seeded the admin keeps the SAME Id (the frontend token / stored ids stay valid).
    private static Guid StableGuid(string key, int i)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(key + ":" + i));
        return new Guid(bytes);
    }

    private static object Placeholder(IProperty p, int i)
    {
        var t = Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType;

        if (t == typeof(Guid)) return Guid.NewGuid();
        if (t == typeof(int)) return -(i + 1);
        if (t == typeof(long)) return (long)-(i + 1);
        if (t == typeof(string)) return "__pending_" + i;

        throw new NotSupportedException("Seed: circular FK of type " + t.Name + " is not supported.");
    }

    // ---------------------------------------------------------------- values

    private object? MakeValue(IEntityType et, IProperty p, int i, string account, bool unique)
    {
        var t = Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType;
        var n = p.Name.ToLowerInvariant();
        var entity = et.ClrType.Name;

        // sane defaults: active users, never locked out, nothing deleted
        if (n.Contains("lockout")) return null;
        if (n.Contains("failed") && (t == typeof(int) || t == typeof(long) || t == typeof(short))) return Convert.ChangeType(0, t);
        if (n.Contains("deleted")) return t == typeof(bool) ? (object)false : null;
        if (t == typeof(bool) && (n == "isactive" || n == "active" || n == "isenabled" || n == "enabled")) return true;

        if (t == typeof(string))
        {
            if (n == "name" && SeedRules.Lookups.TryGetValue(entity, out var names))
                return names[i % names.Length];

            if (n.Contains("password")) return _passwordHash;

            string s;
            var exact = false; // already unique, no suffix needed

            if (n.Contains("accountname") || n.Contains("username") || n == "login") { s = account; exact = true; }
            else if (n.Contains("email")) { s = account + "@almohtaref.local"; exact = true; }
            else if (n.Contains("phone") || n.Contains("mobile")) { s = "09" + (31000000 + i * 137911).ToString("00000000"); exact = true; }
            else if (n.Contains("url") || n.Contains("link")) { s = "https://example.com/seed-" + (i + 1); exact = true; }
            else if (n == "fullname") s = entity == "Person" && i == 0 ? SeedRules.AdminFullName : _f.Name.FullName();
            else if (n.Contains("firstname")) s = _f.Name.FirstName();
            else if (n.Contains("lastname") || n.Contains("familyname")) s = _f.Name.LastName();
            else if (n.Contains("address")) s = _f.Address.StreetAddress();
            else if (n.Contains("description") || n.Contains("note") || n.Contains("comment") || n.Contains("message")) s = "بيانات تجريبية " + (i + 1);
            else if (n.Contains("name") || n.Contains("title")) s = _f.Lorem.Word() + " " + (i + 1);
            else s = _f.Lorem.Sentence(3);

            if (unique && !exact) s += " " + (i + 1);

            var max = p.GetMaxLength();
            return max.HasValue && s.Length > max.Value ? s[..max.Value] : s;
        }

        if (t == typeof(Guid)) return StableGuid(entity + "." + p.Name, i);
        if (t == typeof(int)) return unique ? i + 1 : _f.Random.Int(1, 100);
        if (t == typeof(long)) return unique ? (long)(i + 1) : _f.Random.Long(1, 1000);
        if (t == typeof(short)) return (short)(unique ? i + 1 : _f.Random.Int(1, 100));
        if (t == typeof(byte)) return (byte)(unique ? i + 1 : _f.Random.Int(1, 100));
        if (t == typeof(decimal)) return Math.Round(_f.Random.Decimal(100, 5000), 2);
        if (t == typeof(double)) return Math.Round(_f.Random.Double(1, 1000), 2);
        if (t == typeof(float)) return (float)Math.Round(_f.Random.Double(1, 1000), 2);
        if (t == typeof(bool)) return _f.Random.Bool();
        // dates are relative to a FIXED reference date, so they are the same on every run
        if (t == typeof(DateTime)) return _f.Date.Recent(90, RefDate);
        if (t == typeof(DateTimeOffset)) return new DateTimeOffset(_f.Date.Recent(90, RefDate));
        if (t == typeof(DateOnly)) return DateOnly.FromDateTime(_f.Date.Recent(90, RefDate));
        if (t == typeof(TimeOnly)) return TimeOnly.FromDateTime(_f.Date.Recent(1, RefDate));
        if (t == typeof(TimeSpan)) return TimeSpan.FromMinutes(_f.Random.Int(1, 600));
        if (t == typeof(byte[])) return _f.Random.Bytes(16);

        if (t.IsEnum)
        {
            var all = Enum.GetValues(t);
            return all.GetValue(_f.Random.Int(0, all.Length - 1));
        }

        return null; // complex types: left to the entity default
    }
}