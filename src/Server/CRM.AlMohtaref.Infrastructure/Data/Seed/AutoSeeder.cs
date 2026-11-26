using System.Reflection;
using System.Runtime.CompilerServices;
using Bogus;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
// aliases only: importing the whole Tasks namespace would clash with System.Threading.Tasks.Task / TaskStatus
using DomainTask = CRM.AlMohtaref.Domain.Entities.Tasks.Task;
using StudentTask = CRM.AlMohtaref.Domain.Entities.Tasks.StudentTask;
using TaskState = CRM.AlMohtaref.Domain.Entities.Tasks.TaskStatus;
using TaskType = CRM.AlMohtaref.Domain.Entities.Tasks.TaskType;

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
// - SeedRules.FirstRowOnly: a combination (admin = manager + administration) that only row 0 may have
// - Task / StudentTask are NOT random: they are built from the lead status of every student (see "tasks" below)
// - Campaign.TaskId is filled after the tasks exist (Campaign must be seeded before Task, because the leads
//   need it), so every Campaign points to its own Campaign task
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

            var created = IsTaskEntity(et) ? await FillTaskEntityAsync(et) : await FillAsync(et);
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
               && et.GetViewName() == null
               && et.ClrType.Name != "UserRefreshToken";
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

        // Task is built from users / students / contacts / campaigns / leads: they must be seeded first.
        // Added BEFORE the optional edges below, so an optional edge can never close a cycle through Task
        // (this is also why Campaign.TaskId is filled afterwards, see LinkCampaignsAsync).
        foreach (var e in all.Where(x => x.ClrType.Name == "Task"))
            foreach (var d in all.Where(x => x.ClrType.Name is "User" or "Student" or "Contact" or "Campaign" or "CampaignLead"))
                deps[e].Add(d);

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
            var picked = new Dictionary<string, Row>(); // FK property name -> chosen parent row

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
                picked[fk.Properties[0].Name] = parent;
                for (var j = 0; j < fk.Properties.Count; j++)
                    values[fk.Properties[j]] = parent[fk.PrincipalKey.Properties[j].Name];
            }

            // SeedRules.FirstRowOnly: only row 0 (the admin) may be "manager + administration".
            // Any other row with that combination keeps its role and takes another department.
            if (i > 0)
            {
                foreach (var rule in SeedRules.FirstRowOnly)
                {
                    if (rule.Entity != et.ClrType.Name) continue;
                    if (!picked.TryGetValue(rule.RoleFk, out var first) || !picked.TryGetValue(rule.DepartmentFk, out var second)) continue;
                    if (first.GetValueOrDefault("Name") as string != rule.RoleName || second.GetValueOrDefault("Name") as string != rule.DepartmentName) continue;

                    var fkB = normal.First(k => k.Properties[0].Name == rule.DepartmentFk);
                    var alternative = _rows[fkB.PrincipalEntityType]
                        .Where(r => r.GetValueOrDefault("Name") as string != rule.DepartmentName)
                        .ToList();
                    if (alternative.Count == 0) continue;

                    var replacement = alternative[i % alternative.Count];
                    for (var j = 0; j < fkB.Properties.Count; j++)
                        values[fkB.Properties[j]] = replacement[fkB.PrincipalKey.Properties[j].Name];
                }
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

            // UserPermission.Role is an enum (NOT a foreign key), so FirstRow / FirstRowOnly cannot reach it.
            // Row 0 (the admin's first permission) must be (Manager, Administration).
            // Any other row that would end up (Manager, Administration) keeps Administration but becomes Employee,
            // so that combination stays unique to the admin.
            if (et.ClrType.Name == "UserPermission")
            {
                var roleProp = et.GetProperties().FirstOrDefault(p => p.Name == "Role");
                var deptIdProp = et.GetProperties().FirstOrDefault(p => p.Name == "DepartmentId");
                if (roleProp != null && deptIdProp != null)
                {
                    var deptEntityType = _seedable.FirstOrDefault(e => e.ClrType.Name == "Department");
                    Guid? adminDeptId = null;
                    if (deptEntityType != null && _rows.TryGetValue(deptEntityType, out var deptRows))
                        adminDeptId = deptRows.FirstOrDefault(r => r.TryGetValue("Name", out var nm) && nm as string == "الإدارة")?["Id"] as Guid?;

                    if (adminDeptId.HasValue && values.TryGetValue(deptIdProp, out var deptIdObj) && deptIdObj is Guid deptId)
                    {
                        if (i == 0)
                        {
                            values[roleProp] = Role.Manager;
                            values[deptIdProp] = adminDeptId.Value;
                        }
                        else if (deptId == adminDeptId.Value)
                        {
                            values[roleProp] = Role.Employee;
                        }
                    }
                }
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

            if (n == "title" && SeedRules.Titles.TryGetValue(entity, out var titles))
                return titles[i % titles.Length];

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

        // lead statuses: every status is represented, and every campaign gets a MIX of statuses.
        // Leads go round-robin over the campaigns (row % campaigns), so a plain "i % 10" would give all the
        // leads of one campaign the same status. The shift (i / 10) breaks that link.
        if (t.IsEnum && t.Name == "LeadStatus")
        {
            var statuses = Enum.GetValues(t);
            return statuses.GetValue((i + i / 10) % statuses.Length);
        }

        if (t.IsEnum)
        {
            var all = Enum.GetValues(t);
            return all.GetValue(_f.Random.Int(0, all.Length - 1));
        }

        return null; // complex types: left to the entity default
    }

    // ---------------------------------------------------------------- tasks
    // Tasks are generated FROM the lead status, so the data is always consistent.
    //
    // Hierarchy (matches the API):
    //   Campaign task            one per Campaign row; Campaign.TaskId points to it (see LinkCampaignsAsync)
    //     Contact / FollowUp / Confirmation / Placement task     ParentTaskId = the Campaign task
    //       SubTask            one per contact; ParentTaskId = the stage task; linked to that contact's students
    //   Normal                 standalone, no sub-tasks
    //   Issue                  holds sub-tasks (contacts only, no students are shown for an issue)
    //
    // Which stage tasks a student gets, from the lead status of the student IN THAT CAMPAIGN:
    //   New        -> nothing, or a Contact task still pending
    //   Contacting -> Contact in progress
    //   ContactFailed / Responded -> Contact done (Responded also has a FollowUp pending)
    //   FollowingUp -> FollowUp in progress, FollowUpFailed -> FollowUp done
    //   Confirming -> Confirmation in progress, Confirmed -> done (+ Placement pending)
    //   Assigning -> Placement in progress, Assigned -> done
    // The Campaign task itself: Completed when all its stage tasks are done, Pending when all are pending,
    // otherwise InProgress; its dates cover its stage tasks.
    //
    // Creators / executors are employees (every user except the admin), always given to the one who has the
    // fewest tasks so far. Workflow tasks cannot be multiplied (they mirror the lead statuses), so extra
    // Normal / Issue tasks are added until every employee is assigned at least
    // SeedRules.MinTasksPerEmployee tasks (a parent and each of its sub-tasks count as one each).
    // AssignedToUserId is non-nullable, so EVERY task (including pending ones) gets an executor.

    private sealed record PlanTask(Guid Id, Guid? ParentId, string Title, string? Description, TaskType Type,
        TaskState Status, DateTime Start, DateTime End, Guid CreatedBy, Guid ExecutedBy);

    private List<PlanTask>? _taskPlan;
    private readonly List<(Guid Task, Guid Student)> _studentTaskPlan = new();
    private readonly List<(Guid Campaign, Guid Task)> _campaignTaskPlan = new();

    private static bool IsTaskEntity(IEntityType et)
    {
        return et.ClrType == typeof(DomainTask) || et.ClrType == typeof(StudentTask);
    }

    private async Task<List<Row>> FillTaskEntityAsync(IEntityType et)
    {
        _taskPlan ??= BuildTaskPlan();
        var entries = new List<EntityEntry>();

        if (et.ClrType == typeof(DomainTask))
        {
            foreach (var t in _taskPlan)
                entries.Add(_db.Add(new DomainTask
                {
                    Id = t.Id,
                    ParentTaskId = t.ParentId,
                    Title = t.Title,
                    Description = t.Description,
                    TaskType = t.Type,
                    TaskStatus = t.Status,
                    StartTime = t.Start,
                    EndTime = t.End,
                    CreatedByUserId = t.CreatedBy,
                    AssignedToUserId = t.ExecutedBy
                }));
        }
        else
        {
            foreach (var (taskId, studentId) in _studentTaskPlan)
            {
                var entry = _db.Add(new StudentTask
                {
                    Id = StableGuid("StudentTask", entries.Count),
                    TaskId = taskId,
                    StudentId = studentId
                });
                entry.Property("TaskType").CurrentValue = TaskType.SubTask;
                entries.Add(entry);
            }
        }

        if (entries.Count == 0) return new List<Row>();

        await _db.SaveChangesAsync(_ct);

        if (et.ClrType == typeof(DomainTask))
            await LinkCampaignsAsync();

        return entries.Select(Snapshot).ToList();
    }

    // Campaign.TaskId (optional FK) cannot be filled while the campaigns are inserted: the tasks do not exist yet
    // (they are built from the leads, which need the campaigns). So it is filled here, once the tasks are saved.
    private async Task LinkCampaignsAsync()
    {
        var campaign = _seedable.FirstOrDefault(e => e.ClrType.Name == "Campaign");
        var taskIdProp = campaign?.FindProperty("TaskId");
        if (campaign == null || taskIdProp == null || _campaignTaskPlan.Count == 0) return;

        var store = StoreObjectIdentifier.Table(campaign.GetTableName()!, campaign.GetSchema());
        var table = Table(campaign);
        var taskColumn = taskIdProp.GetColumnName(store);
        var idColumn = campaign.FindPrimaryKey()!.Properties[0].GetColumnName(store);
        var campaignRows = RowsOf("Campaign");

        foreach (var (campaignId, taskId) in _campaignTaskPlan)
        {
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE " + table + " SET [" + taskColumn + "] = {0} WHERE [" + idColumn + "] = {1}",
                new object[] { taskId, campaignId }, _ct);

            var row = campaignRows.FirstOrDefault(r => r.TryGetValue("Id", out var cid) && cid is Guid g && g == campaignId);
            if (row != null) row["TaskId"] = taskId;
        }

        Console.WriteLine("[seed] Campaign: " + _campaignTaskPlan.Count + " campaigns linked to their Campaign task");
    }

    private List<Row> RowsOf(string clrName)
    {
        return _rows.FirstOrDefault(kv => kv.Key.ClrType.Name == clrName).Value ?? new List<Row>();
    }

    private static List<(TaskType Stage, TaskState Status)> L(params (TaskType, TaskState)[] steps)
    {
        return steps.Select(s => (s.Item1, s.Item2)).ToList();
    }

    // lead status number (LeadStatus enum) -> tasks that must exist for that student
    private static List<(TaskType Stage, TaskState Status)> Chain(int lead, int index)
    {
        const TaskType C = TaskType.Contact, F = TaskType.FollowUp, K = TaskType.Confirmation, P = TaskType.Placement;
        const TaskState pend = TaskState.Pending, prog = TaskState.InProgress, done = TaskState.Completed;

        return lead switch
        {
            1 => index % 2 == 0 ? L() : L((C, pend)),                 // New
            2 => L((C, prog)),                                        // Contacting
            3 => L((C, done)),                                        // ContactFailed
            4 => L((C, done), (F, pend)),                             // Responded
            5 => L((C, done), (F, prog)),                             // FollowingUp
            6 => L((C, done), (F, done)),                             // FollowUpFailed
            7 => L((C, done), (F, done), (K, prog)),                  // Confirming
            8 => L((C, done), (F, done), (K, done), (P, pend)),       // Confirmed
            9 => L((C, done), (F, done), (K, done), (P, prog)),       // Assigning
            10 => L((C, done), (F, done), (K, done), (P, done)),      // Assigned
            _ => L()
        };
    }

    // (student, contact, campaign, lead status). The status lives on CampaignLead: one row per student per campaign.
    private List<(Guid Student, Guid Contact, Guid Campaign, int Status)> CollectLeads(Dictionary<Guid, Guid> contactOfStudent)
    {
        var result = new List<(Guid, Guid, Guid, int)>();
        var seen = new HashSet<(Guid, Guid)>(); // one lead per student per campaign

        foreach (var r in RowsOf("CampaignLead"))
        {
            if (r.TryGetValue("StudentId", out var s) && s is Guid sid
                && r.TryGetValue("CampaignId", out var c) && c is Guid cid
                && r.TryGetValue("Status", out var st) && st != null
                && contactOfStudent.TryGetValue(sid, out var contactId)
                && seen.Add((sid, cid)))
            {
                result.Add((sid, contactId, cid, Convert.ToInt32(st)));
            }
        }

        return result;
    }

    // Completed tasks sit in the past (earlier stage = earlier dates), in-progress around RefDate, pending in the future.
    private static (DateTime Start, DateTime End) Window(TaskState status, int stage, int chunk)
    {
        var c = chunk % 3;
        var start = status switch
        {
            TaskState.Completed => RefDate.AddDays(-60 + stage * 15 + c),
            TaskState.Pending => RefDate.AddDays(1 + c),
            _ => RefDate.AddDays(-5 - c)
        };
        var end = status switch
        {
            TaskState.Completed => start.AddDays(10),
            TaskState.Pending => start.AddDays(14),
            _ => RefDate.AddDays(5 + c)
        };
        return (start.Date, end.Date);
    }

    private List<PlanTask> BuildTaskPlan()
    {
        var plan = new List<PlanTask>();
        _studentTaskPlan.Clear();
        _campaignTaskPlan.Clear();

        var users = RowsOf("User").Select(r => (Guid)r["Id"]!).ToList();
        var students = RowsOf("Student").Where(r => r.TryGetValue("ContactId", out var c) && c is Guid).ToList();
        if (users.Count == 0 || students.Count == 0) return plan;

        var staff = users.Count > 1 ? users.Skip(1).ToList() : users; // employees, not the admin
        var contactNames = RowsOf("Contact").ToDictionary(r => (Guid)r["Id"]!, r => r.TryGetValue("FullName", out var fn) ? fn as string : null);
        var contactOfStudent = students.ToDictionary(r => (Guid)r["Id"]!, r => (Guid)r["ContactId"]!);
        var studentsByContact = students.GroupBy(r => (Guid)r["ContactId"]!)
            .ToDictionary(g => g.Key, g => g.Select(r => (Guid)r["Id"]!).ToList());

        var campaigns = RowsOf("Campaign")
            .Where(r => r.TryGetValue("Id", out var cid) && cid is Guid)
            .Select(r => (Id: (Guid)r["Id"]!, Title: r.TryGetValue("Title", out var ct) ? ct as string : null))
            .ToList();

        // tasks created / executed so far per employee (parent + each sub-task = 1 each)
        var created = staff.ToDictionary(s => s, _ => 0);
        var executed = staff.ToDictionary(s => s, _ => 0);

        // the employee with the fewest tasks; ties go to the first one (OrderBy is stable), so it is deterministic
        Guid Least(Dictionary<Guid, int> load) => staff.OrderBy(s => load[s]).First();

        string Name(Guid contact) => contactNames.TryGetValue(contact, out var nm) && !string.IsNullOrWhiteSpace(nm) ? nm : contact.ToString()[..8];
        string Cut(string text) => text.Length > 200 ? text[..200] : text;

        // one task + its sub-tasks. parentTaskId is the parent of the task itself (the Campaign task for stage tasks).
        void AddBatch(Guid? parentTaskId, TaskType type, TaskState status, string title, int stage, int chunk, List<(string Title, List<Guid> Students)> subs)
        {
            var (start, end) = Window(status, stage, chunk);
            var weight = 1 + subs.Count;

            var createdBy = Least(created);
            created[createdBy] += weight;

            // AssignedToUserId is non-nullable: every task gets an executor, even pending ones.
            var executedBy = Least(executed);
            executed[executedBy] += weight;

            var parentId = StableGuid("Task", plan.Count);
            plan.Add(new PlanTask(parentId, parentTaskId, Cut(title), "بيانات تجريبية", type, status, start, end, createdBy, executedBy));

            foreach (var (subTitle, studentIds) in subs)
            {
                var id = StableGuid("Task", plan.Count);
                plan.Add(new PlanTask(id, parentId, Cut(subTitle), null, TaskType.SubTask, status, start, end, createdBy, executedBy));
                foreach (var sid in studentIds) _studentTaskPlan.Add((id, sid));
            }
        }

        var leadsByCampaign = CollectLeads(contactOfStudent).ToLookup(l => l.Campaign);
        var stages = new[] { TaskType.Contact, TaskType.FollowUp, TaskType.Confirmation, TaskType.Placement };
        var states = new[] { TaskState.Completed, TaskState.InProgress, TaskState.Pending };
        var index = 0;

        // ---- one Campaign task per Campaign. Its stage tasks are built from the leads OF THAT CAMPAIGN:
        //      stage task (parent = campaign task) -> one sub-task per contact -> that contact's students
        foreach (var (campaignId, campaignTitle) in campaigns)
        {
            var label = string.IsNullOrWhiteSpace(campaignTitle) ? campaignId.ToString()[..8] : campaignTitle;

            // the campaign task is added first (its id is the parent of the stage tasks);
            // status / dates / executor are filled below, once its stage tasks are known
            var campaignIndex = plan.Count;
            var campaignTaskId = StableGuid("Task", campaignIndex);
            var campaignCreator = Least(created);
            created[campaignCreator]++;
            // placeholder executor (replaced below); never null because AssignedToUserId is non-nullable
            var campaignExecutor0 = Least(executed);
            executed[campaignExecutor0]++;
            plan.Add(new PlanTask(campaignTaskId, null, Cut(SeedRules.TaskTitles["Campaign"] + label), "بيانات تجريبية",
                TaskType.Campaign, TaskState.Pending, RefDate, RefDate, campaignCreator, campaignExecutor0));
            _campaignTaskPlan.Add((campaignId, campaignTaskId));

            var buckets = new Dictionary<(TaskType, TaskState, Guid), List<Guid>>();
            var order = new List<(TaskType Stage, TaskState Status, Guid Contact)>();

            foreach (var lead in leadsByCampaign[campaignId])
                foreach (var (leadStage, leadState) in Chain(lead.Status, index++))
                {
                    var key = (leadStage, leadState, lead.Contact);
                    if (!buckets.TryGetValue(key, out var list))
                    {
                        buckets[key] = list = new List<Guid>();
                        order.Add(key);
                    }
                    list.Add(lead.Student);
                }

            for (var si = 0; si < stages.Length; si++)
                foreach (var st in states)
                {
                    var stage = stages[si];
                    var subs = order.Where(k => k.Stage == stage && k.Status == st)
                        .Select(k => (SeedRules.SubTaskPrefix[stage.ToString()] + Name(k.Contact), buckets[(k.Stage, k.Status, k.Contact)]))
                        .ToList();

                    for (var chunk = 0; chunk * SeedRules.SubTasksPerParent < subs.Count; chunk++)
                        AddBatch(campaignTaskId, stage, st,
                            SeedRules.TaskTitles[stage.ToString()] + " - " + label + " (" + (chunk + 1) + ")", si, chunk,
                            subs.Skip(chunk * SeedRules.SubTasksPerParent).Take(SeedRules.SubTasksPerParent).ToList());
                }

            // finish the campaign task from its stage tasks
            var kids = plan.Where(p => p.ParentId == campaignTaskId).ToList();

            var campaignState = kids.Count == 0 || kids.All(p => p.Status == TaskState.Pending) ? TaskState.Pending
                : kids.All(p => p.Status == TaskState.Completed) ? TaskState.Completed
                : TaskState.InProgress;

            var campaignStart = RefDate.AddDays(1).Date;
            var campaignEnd = RefDate.AddDays(15).Date;
            if (kids.Count > 0)
            {
                campaignStart = kids.Min(p => p.Start);
                campaignEnd = kids.Max(p => p.End);
            }

            // AssignedToUserId is non-nullable: the campaign task keeps the executor chosen above
            // regardless of its final state (the load was already counted when the placeholder was picked).
            plan[campaignIndex] = plan[campaignIndex] with
            {
                Status = campaignState,
                Start = campaignStart,
                End = campaignEnd
            };
        }

        // ---- Normal: standalone, no sub-tasks (the API says a normal task never has sub-tasks).
        // ---- Issue: sub-tasks only (one per contact; the contacts come from the students linked to the sub-task).
        // The first 16 are the fixed set (8 statuses x 2 types); after that more are added, one by one, until
        // every employee executes at least SeedRules.MinTasksPerEmployee tasks.
        var contactIds = studentsByContact.Keys.ToList();
        var generalStatuses = new[]
        {
            TaskState.Pending, TaskState.InProgress, TaskState.Completed, TaskState.OnHold,
            TaskState.Cancelled, TaskState.InProgress, TaskState.Completed, TaskState.Pending
        };

        for (var g = 0; g < 2000; g++)
        {
            if (g >= 16 && staff.All(s => executed[s] >= SeedRules.MinTasksPerEmployee)) break;

            var type = g % 2 == 0 ? TaskType.Normal : TaskType.Issue;
            var k = g / 2; // position inside its type
            var status = generalStatuses[k % generalStatuses.Length];

            var subs = type == TaskType.Normal
                ? new List<(string Title, List<Guid> Students)>()
                : Enumerable.Range(0, 2 + k % 2)
                    .Select(j => contactIds[(g * 3 + j) % contactIds.Count]).Distinct()
                    .Select(cid => (Title: SeedRules.SubTaskPrefix["General"] + Name(cid), Students: studentsByContact[cid]))
                    .ToList();

            AddBatch(null, type, status, SeedRules.TaskTitles[type.ToString()] + " (" + (k + 1) + ")", 0, k, subs);
        }

        return plan;
    }
}