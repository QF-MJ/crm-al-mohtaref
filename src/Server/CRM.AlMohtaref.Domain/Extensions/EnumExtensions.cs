using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Domain.Entities.Tasks;

using TaskStatus = CRM.AlMohtaref.Domain.Entities.Tasks.TaskStatus;

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

        public static string ToTaskTypeString(this TaskType taskType)
        {
            return taskType switch
            {
                TaskType.Normal => "مهمة عادية",
                TaskType.Issue => "مشكلة",
                TaskType.Campaign => "حملة",
                TaskType.Contact => "تواصل",
                TaskType.FollowUp => "متابعة",
                TaskType.Confirmation => "تثبيت",
                TaskType.Placement => "تسكين",
                TaskType.SubTask => "مهمة فرعية",
                _ => taskType.ToString()
            };
        }

        public static string ToTaskStatusString(this TaskStatus taskStatus)
        {
            return taskStatus switch
            {
                TaskStatus.Pending => "قيد الانتظار",
                TaskStatus.InProgress => "قيد التنفيذ",
                TaskStatus.Completed => "مكتملة",
                TaskStatus.Cancelled => "ملغاة",
                TaskStatus.OnHold => "معلقة",
                _ => taskStatus.ToString()
            };
        }
    }
}
