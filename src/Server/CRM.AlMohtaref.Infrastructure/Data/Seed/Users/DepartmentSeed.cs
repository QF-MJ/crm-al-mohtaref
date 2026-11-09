using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed.Users;

// Departments = the departments that appear in the use case actors:
// (sales / marketing / customer service / academic guidance).
internal sealed class DepartmentSeed : EntitySeed<Department>
{
    public const string Sales = "المبيعات";
    public const string Marketing = "التسويق";
    public const string CustomerService = "خدمة العملاء";
    public const string AcademicGuidance = "التوجيه الأكاديمي";

    // the admin works here as a manager
    public const string Administration = "الإدارة";

    public override int Order => 100;

    protected override Task<IReadOnlyList<Department>> CreateAsync(AppDbContext db, CancellationToken ct)
    {
        var names = new[] { Sales, Marketing, CustomerService, AcademicGuidance, Administration };
        return Result(names.Select(n => new Department(n)));
    }
}