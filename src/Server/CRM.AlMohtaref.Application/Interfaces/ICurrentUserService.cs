
namespace CRM.AlMohtaref.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        IReadOnlyCollection<string> Permissions { get; }
        bool IsAuthenticated { get; }
    }
}
