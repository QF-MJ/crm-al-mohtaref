
using System.Security.Claims;
using CRM.AlMohtaref.Application.Interfaces;
using Microsoft.AspNetCore.Http;


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

        public IReadOnlyCollection<string> Roles =>
            User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
