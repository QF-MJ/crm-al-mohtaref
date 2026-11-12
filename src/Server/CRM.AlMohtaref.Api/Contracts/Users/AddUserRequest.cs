namespace CRM.AlMohtaref.Api.Contracts.Users
{
    public class AddUserRequest
    {
        public string AccountName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public List<PermissionRequest>? Permissions { get; set; }
    }
}