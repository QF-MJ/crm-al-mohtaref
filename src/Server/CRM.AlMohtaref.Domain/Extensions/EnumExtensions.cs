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
    }
}
