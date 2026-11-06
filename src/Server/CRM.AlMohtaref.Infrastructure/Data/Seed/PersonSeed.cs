using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

internal static class PersonSeed
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

    // index 0 is the admin, the rest are normal users
    public static List<Person> Create()
    {
        var names = new List<string> { AdminFullName };
        names.AddRange(EmployeeNames);

        // CreatedByUserId is set by DataSeeder once the admin user exists
        return names.Select(n => new Person { FullName = n, CreatedByUserId = 0 }).ToList();
    }
}
