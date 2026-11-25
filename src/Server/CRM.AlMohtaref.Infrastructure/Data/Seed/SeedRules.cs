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
    // Task / StudentTask are not listed: they are built from the lead statuses (see AutoSeeder).
    public static readonly Dictionary<string, int> RowCounts = new()
    {
        ["User"] = 25,      // admin + 24 employees
        ["Contact"] = 30,
        ["Student"] = 45,
        ["Campaign"] = 10,  // each one gets its own Campaign task (Campaign.TaskId)
        ["CampaignLead"] = 45, // every student is a lead; LeadStatus is spread so every campaign has a mix of statuses
        ["UserPermission"] = 35 // >= users: every user gets at least one permission, the first 10 get a second one
    };

    // parent chosen by its Name for the FIRST row of an entity ("Entity.FkProperty" -> parent Name).
    // Row 0 of User is the admin, so its permission is: manager in the administration department.
    public static readonly Dictionary<string, string> FirstRow = new()
    {
        ["UserPermission.RoleId"] = "مدير",
        ["UserPermission.DepartmentId"] = "الإدارة"
    };

    // combination (role + department) that ONLY row 0 may have (the admin).
    // Any other row that would get it keeps its role and moves to another department.
    public static readonly (string Entity, string RoleFk, string RoleName, string DepartmentFk, string DepartmentName)[] FirstRowOnly =
    {
        ("UserPermission", "RoleId", "مدير", "DepartmentId", "الإدارة")
    };

    // every employee (every user except the admin) executes at least this many tasks (sub-tasks count too).
    // Extra Normal / Issue tasks are added until everybody reaches it (see AutoSeeder, "tasks").
    public const int MinTasksPerEmployee = 20;

    // lookup tables with fixed meaningful values: one row per item, written to the "Name" property.
    public static readonly Dictionary<string, string[]> Lookups = new()
    {
        ["Department"] = new[] { "المبيعات", "التسويق", "خدمة العملاء", "التوجيه الأكاديمي", "الإدارة" },

        ["Role"] = new[] { "موظف", "مدير" },

        ["Track"] = new[] { "البرمجة للأطفال", "اللغة الإنجليزية للأطفال" },

        ["CampaginStatus"] = new[] { "مسودة", "نشطة", "متوقفة مؤقتاً", "منتهية", "ملغاة" },

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

    public static readonly Dictionary<string, string[]> Titles = new()
    {
        ["Campaign"] = new[]
        {
            "برمجة_2026_01_15",
            "انجليزي_2026_02_03",
            "برمجة_2026_03_22",
            "انجليزي_2026_04_10",
            "برمجة_2026_05_28",
            "انجليزي_2026_06_17",
            "برمجة_2026_07_05",
            "انجليزي_2026_08_24",
            "برمجة_2026_09_11",
            "انجليزي_2026_10_30"
        }
    };

    // ---------------------------------------------------------------- tasks

    // sub-tasks per parent task (workflow tasks are split into batches of this size)
    public const int SubTasksPerParent = 5;

    // parent task titles, key = TaskType name.
    // "Campaign" is a PREFIX: the Campaign task of each campaign is named prefix + campaign title.
    // The stage tasks (Contact / FollowUp / Confirmation / Placement) get " - campaign title (n)" appended.
    public static readonly Dictionary<string, string> TaskTitles = new()
    {
        ["Contact"] = "تواصل أولي مع عملاء جدد",
        ["FollowUp"] = "متابعة العملاء بعد الاستجابة",
        ["Confirmation"] = "تثبيت العميل بالكورس",
        ["Placement"] = "تسكين الطلاب في المجموعات المناسبة",
        ["Normal"] = "مراجعة تقارير الأداء الأسبوعية",
        ["Issue"] = "مشكلة في دخول الطالب للمنصة",
        ["Campaign"] = "إدارة الحملة: "
    };

    // sub-task title = prefix + contact name (General = under Issue tasks)
    public static readonly Dictionary<string, string> SubTaskPrefix = new()
    {
        ["Contact"] = "تواصل مع: ",
        ["FollowUp"] = "متابعة: ",
        ["Confirmation"] = "تثبيت التسجيل: ",
        ["Placement"] = "تسكين طلاب: ",
        ["General"] = "متابعة فرعية لطلاب ولي الأمر: "
    };
}