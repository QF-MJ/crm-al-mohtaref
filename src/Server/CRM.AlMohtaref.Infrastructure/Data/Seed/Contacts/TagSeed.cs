using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Contacts;

// Tags come from the answers of the lead form questions
// (programming course for kids + English course for kids).
internal sealed class TagSeed : EntitySeed<Tag>
{
    // which course the lead came for
    public const string InterestedProgramming = "مهتم بكورس البرمجة";
    public const string InterestedEnglish = "مهتم بكورس اللغة الإنجليزية";

    // هل_يتوفر_جهاز_للطفل؟
    public const string HasDevice = "يتوفر جهاز للطفل";
    public const string NoDevice = "لا يتوفر جهاز للطفل";

    // هل_توافق_على_أن_يحضر_طفلك_الجلسات_أونلاين؟
    public const string AgreesOnline = "يوافق على الدراسة أونلاين";

    // متى_ترغب_بالبدء؟  (answer "فوراً" = hot lead, called within 15-30 minutes)
    public const string StartNow = "يرغب بالبدء فوراً";
    public const string HotLead = "عميل ساخن";

    public override int Order => 200;

    protected override Task<IReadOnlyList<Tag>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new[]
        {
            InterestedProgramming, InterestedEnglish,

            // عمر_الطفل؟
            "العمر 6-9 سنوات", "العمر 10-14 سنة",

            HasDevice, NoDevice,

            // هل_لدى_الطفل_خبرة_سابقة_بالبرمجة؟
            "لديه خبرة سابقة بالبرمجة", "مبتدئ في البرمجة",

            // الهدف_من_التسجيل (programming)
            "الهدف: تنمية التفكير المنطقي", "الهدف: استغلال الوقت", "الهدف: التخصص مستقبلاً",

            // الهدف_من_التسجيل (english)
            "الهدف: التقوية في المدرسة", "الهدف: تطوير المحادثة", "الهدف: التأسيس من الصفر",

            StartNow, HotLead, AgreesOnline,

            // مستوى_الطفل_باللغة_الانجليزية
            "مستوى الإنجليزية: مبتدئ", "مستوى الإنجليزية: متوسط", "مستوى الإنجليزية: محادثة",

            // ما النمط الذي تفضله لطفلك في هذا البرنامج؟
            "النمط: جلسات فردية", "النمط: مجموعات تفاعلية", "النمط: دمج خاص"
        };

        return Result(names.Select(n => new Tag(n)));
    }
}