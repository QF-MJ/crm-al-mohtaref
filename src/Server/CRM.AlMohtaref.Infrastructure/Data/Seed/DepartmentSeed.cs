using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

// Departments = the departments that appear in the use case actors:
// (sales / marketing / customer service / academic guidance).
internal static class DepartmentSeed
{
    public const string Sales = "المبيعات";
    public const string Marketing = "التسويق";
    public const string CustomerService = "خدمة العملاء";
    public const string AcademicGuidance = "التوجيه الأكاديمي";

    public static readonly string[] Names = { Sales, Marketing, CustomerService, AcademicGuidance };

    public static List<Department> Create()
    {
        return Names.Select(n => new Department(n)).ToList();
    }
}
