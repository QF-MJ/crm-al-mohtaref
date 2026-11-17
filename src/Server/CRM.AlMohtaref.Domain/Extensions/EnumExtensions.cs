using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string ToRoleString(this Role role)
        {
            return role switch
            {
                Role.Manager => "مدير",
                Role.Employee => "موظف",
                _ => role.ToString()
            };
        }

        public static string ToLeadStatusString(this LeadStatus status)
        {
            return status switch
            {
                LeadStatus.New => "جديد",
                LeadStatus.Contacting => "قيد التواصل",
                LeadStatus.ContactFailed => "فشل التواصل",
                LeadStatus.Responded => "حدثت استجابة",
                LeadStatus.FollowingUp => "قيد المتابعة",
                LeadStatus.FollowUpFailed => "فشلت المتابعة",
                LeadStatus.Confirming => "قيد التثبيت",
                LeadStatus.Confirmed => "تم التثبيت",
                LeadStatus.Assigning => "قيد التسكين",
                LeadStatus.Assigned => "تم التسكين",
                _ => status.ToString()
            };
        }
    }
}
