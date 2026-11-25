using CRM.AlMohtaref.Domain.Enums;

namespace CRM.AlMohtaref.Domain.Extensions;

public static class CampaignStatusExtensions
{
    public static string ToArabicName(this CampaignStatus status) => status switch
    {
        CampaignStatus.New => "جديدة",
        CampaignStatus.Approved => "معتمدة",
        CampaignStatus.InProgress => "قيد التنفيذ",
        CampaignStatus.Completed => "مكتملة",
        CampaignStatus.NeedsModification => "تحتاج إلى تعديل",
        CampaignStatus.Cancelled => "ملغاة",
        CampaignStatus.Rejected => "مرفوضة",
        _ => "غير معروف"
    };
}
