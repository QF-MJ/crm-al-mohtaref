using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginDto>
    {
        private readonly IUserRepository _user;
        private readonly IPasswordHasher _password;
        private readonly ITokenService _token;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(IUserRepository user, IPasswordHasher password, ITokenService token, IUnitOfWork unitOfWork)
        {
            _user = user;
            _password = password;
            _token = token;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
 
            var user = await _user.GetByAccountNameAsync(request.AccountName, cancellationToken);

            if (user == null|| !user.IsActive)
                throw new UnauthorizedException("كلمة السر أو اسم الحساب غير صحيحين");

            if (user.LockoutEnd != null && user.LockoutEnd >= DateTime.Now)
            {
                throw new ForbiddenException($"الحساب مقفل حالياً. يرجى المحاولة مجدداً بعد: {user.LockoutEnd}");
            }

            if (!_password.VerifyPassword(request.Password, user.PasswordHash))
            {
                user.RegisterFailedLogin();

               
                if (user.FailedLoginCount == 5)
                {
                    user.LockAccount(DateTime.UtcNow.AddDays(1));
                }

                await _unitOfWork.SaveChangesAsync();

                throw new UnauthorizedException("كلمة السر او اسم الحساب غير صحيحين");
            }

 
            user.ResetFailedLogin();

            await _unitOfWork.SaveChangesAsync();

            string token = await _token.GenerateAccessToken(user);

            List<PermissionDto> permissions = user.UserPermissions
                .Select(p => new PermissionDto { Id = p.Id, RoleName = p.Role.Name, DepartmentName = p.Department.Name })
                .ToList();

            return new LoginDto
            (
                token,
                null,
                _token.GetExpireAt(),
                user.Id,
                user.Person.FullName ?? string.Empty,
                permissions
            );
        }
    }
}
