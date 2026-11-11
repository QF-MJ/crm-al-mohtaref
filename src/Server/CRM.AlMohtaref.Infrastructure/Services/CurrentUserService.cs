
using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Security.Claims;


namespace CRM.AlMohtaref.Infrastructure.Services
{

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId =>
            Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public string? Email => User?.FindFirstValue(ClaimTypes.Email);

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public List<UserPermission>? Permissions 
        {
            get
            {
               return  User?.FindAll("permission").Select(p => p.Value.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .Select(parts => new UserPermission(parts[0], parts[1]))
                        .ToList();
            }
        }

        public bool HasDepartment(string? department)
        {
            if (department == null)
                return false;

            return Permissions?.Any(p => p.Department.Name == department) ?? false;
        }

        public bool HasAnyDepartment(IEnumerable<PermissionDto>? permissions)
        {
            if (Permissions is null || permissions is null)
                return false;

            var targetDepartments = permissions
                .Select(pr => pr.DepartmentName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            if (targetDepartments.Count == 0)
                return false;

            return Permissions.Any(p => p.Department?.Name != null && targetDepartments.Contains(p.Department.Name));
        }

    }
}
