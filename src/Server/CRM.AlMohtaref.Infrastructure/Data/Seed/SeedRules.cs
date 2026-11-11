namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// The ONLY file you edit by hand for fake data. Everything else is read from the EF model.
// Keys are the entity CLR class names (Department, Role, Track, Tag ...).
internal static class SeedRules
{
    // password of every fake user, hashed with your own PasswordHasher
    public const string PlainPassword = "12345678";

    public const string AdminAccount = "admin";
    public const string AdminFullName = "مدير النظام";

    public const int DefaultRows = 20;

    // rows per entity (default = DefaultRows). The generator may raise a number when
    // a one-to-one relation needs more parents (e.g. Person must cover User + Contact + Student).
    public static readonly Dictionary<string, int> RowCounts = new()
    {
        ["User"] = 25,      // admin + 24 employees
        ["Contact"] = 30,
        ["Student"] = 45,
        ["Campaign"] = 10,
        ["UserPermission"] = 35 // >= users: every user gets at least one permission, the first 10 get a second one
    };

    // parent chosen by its Name for the FIRST row of an entity ("Entity.FkProperty" -> parent Name).
    // Row 0 of User is the admin, so its permission is: manager in the administration department.
    public static readonly Dictionary<string, string> FirstRow = new()
    {
        ["UserPermission.RoleId"] = "مدير",
        ["UserPermission.DepartmentId"] = "الإدارة"
    };

    // lookup tables with fixed meaningful values: one row per item, written to the "Name" property.
    public static readonly Dictionary<string, string[]> Lookups = new()
    {
        ["Department"] = new[] { "المبيعات", "التسويق", "خدمة العملاء", "التوجيه الأكاديمي", "الإدارة" },

        ["Role"] = new[] { "موظف", "مدير" },

        ["Track"] = new[] { "البرمجة للأطفال", "اللغة الإنجليزية للأطفال" },

        ["CampaginStatus"] = new[] { "مسودة", "نشطة", "متوقفة مؤقتاً", "منتهية", "ملغاة" },

        ["LeadStatus"] = new[] { "جديد", "تم التواصل", "مسجل", "غير مهتم" },

        ["Tag"] = new[]
        {
            "مهتم بكورس البرمجة", "مهتم بكورس اللغة الإنجليزية",
            "العمر 6-9 سنوات", "العمر 10-14 سنة",
            "يتوفر جهاز للطفل", "لا يتوفر جهاز للطفل",
            "لديه خبرة سابقة بالبرمجة", "مبتدئ في البرمجة",
            "الهدف: تنمية التفكير المنطقي", "الهدف: استغلال الوقت", "الهدف: التخصص مستقبلاً",
            "الهدف: التقوية في المدرسة", "الهدف: تطوير المحادثة", "الهدف: التأسيس من الصفر",
            "يرغب بالبدء فوراً", "عميل ساخن", "يوافق على الدراسة أونلاين",
            "مستوى الإنجليزية: مبتدئ", "مستوى الإنجليزية: متوسط", "مستوى الإنجليزية: محادثة",
            "النمط: جلسات فردية", "النمط: مجموعات تفاعلية", "النمط: دمج خاص"
        }
    };
}