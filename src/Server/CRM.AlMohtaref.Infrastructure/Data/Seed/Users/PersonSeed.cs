using CRM.AlMohtaref.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Users;

// The people behind the users (index 0 = admin, then 24 employees).
//
// Person.CreatedByUserId points to User and User.PersonId points to Person (circular FK).
// This seed switches the Person FK checks off before inserting; UserSeed (the next step of the
// same circle) fixes CreatedByUserId and switches the checks back on.
internal sealed class PersonSeed : EntitySeed<Person>
{
    public const string AdminFullName = "مدير النظام";

    private static readonly string[] EmployeeNames =
    {
        "أحمد خالد الحسن", "سارة محمد العلي", "عمر يوسف حداد", "ليلى حسام الدين",
        "محمد أنس قباني", "رنا سمير الخطيب", "يوسف عدنان درويش", "هبة الله مصطفى",
        "كريم وائل شاهين", "دانا فادي منصور", "طارق نزار السيد", "ميساء جمال ديب",
        "باسل رامي حمدان", "نور الهدى بكري", "زياد ماهر عيسى", "لانا عماد يونس",
        "حسين علي إبراهيم", "مرح سليم الأحمد", "فراس غسان نصر", "ريم أيمن سلوم",
        "ماهر بشير العمر", "جود طلال مراد", "سامر هشام عباس", "آلاء رياض ناصر"
    };

    public override int Order => 120;

    public override async Task SeedAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Set<Person>().AnyAsync(ct))
            return;

        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE " + SeedHelpers.TableName<Person>(db) + " NOCHECK CONSTRAINT ALL", ct);

        await base.SeedAsync(db, ct);
    }

    protected override Task<IReadOnlyList<Person>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new List<string> { AdminFullName };
        names.AddRange(EmployeeNames);

        // CreatedByUserId is fixed by UserSeed once the admin user exists
        return Result(names.Select(n => new Person { FullName = n, CreatedByUserId = 0 }));
    }
}