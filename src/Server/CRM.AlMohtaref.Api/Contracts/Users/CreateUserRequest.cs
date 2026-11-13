using CRM.AlMohtaref.Application.Features.Users;

namespace CRM.AlMohtaref.Api.Contracts.Users
{
    public class CreateUserRequest
    {
        public string AccountName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public List<PermissionRequest>? Permissions { get; set; }


        public CreateUserCommand ToCommand() => new CreateUserCommand(
            Email,
            AccountName,
            FullName,
            Password,
            Permissions?.Select(p => new PermissionCommand(p.RoleId, p.DepartmentId)).ToList()
        );

    }
}