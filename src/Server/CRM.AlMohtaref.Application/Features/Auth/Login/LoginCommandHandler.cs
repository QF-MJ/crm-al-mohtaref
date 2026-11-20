using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;
using BCrypt.Net;
using System.Security.Claims;


namespace CRM.AlMohtaref.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginDto>
    {
        private readonly IUserRepository _user;
        private readonly IPasswordHasher _password;
        private readonly ITokenService _token;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;

        public LoginCommandHandler(IUserRepository user, IPasswordHasher password, ITokenService token, IUnitOfWork unitOfWork , IUserRefreshTokenRepository userRefreshTokenRepository)
        {
            _user = user;
            _password = password;
            _token = token;
            _unitOfWork = unitOfWork;
            _refreshTokenRepository = userRefreshTokenRepository;
        }

        public async Task<LoginDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
 
            var user = await _user.GetByAccountNameAsync(request.AccountName, cancellationToken);

            if (user == null|| !user.IsActive)
                throw new UnauthorizedException("كلمة السر أو اسم الحساب غير صحيحين");

            if (user.LockoutEnd != null && user.LockoutEnd >= DateTime.UtcNow)
            {
                throw new ForbiddenException($"الحساب مقفل حالياً. يرجى المحاولة مجدداً بعد: {user.LockoutEnd}");
            }

            if (!_password.VerifyPassword(request.Password, user.PasswordHash))
            {
                user.RegisterFailedLogin();

               
                if (user.FailedLoginCount >= 5)
                {
                    user.LockAccount(DateTime.UtcNow.AddDays(1));
                }

                await _unitOfWork.SaveChangesAsync();

                throw new UnauthorizedException("كلمة السر أو اسم الحساب غير صحيحين");
            }

            string token = await _token.GenerateAccessToken(user);

            List<PermissionDto> permissions = user.UserPermissions
                .Select(p => new PermissionDto { Id = p.Id, RoleName = p.Role == Role.Manager ? "مدير" : "موظف", DepartmentName = p.Department.Name })
                .ToList();


            var refreshToken = _token.GenerateRefreshToken();

            //Store refresh token securely (hash + expiry + not revoked)
            var refreshTokenHash = _password.HashPassword(refreshToken);
            Guid RefreshId = Guid.NewGuid();
            refreshToken = RefreshId.ToString() + "." + refreshToken;
            var refreshTokenExpiredAt = DateTime.UtcNow.AddDays(7);


            _refreshTokenRepository.Add(new UserRefreshToken(
                RefreshId,
                user.Id,
                refreshTokenHash,
                refreshTokenExpiredAt,
                null));

            user.ResetFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginDto
            (
                token,
                refreshToken,
                _token.GetExpireAt(),
                user.Id,
                user.FullName ?? string.Empty,
                permissions
            );
        }
    }
}
