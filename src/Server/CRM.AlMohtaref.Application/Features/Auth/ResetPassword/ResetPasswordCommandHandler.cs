using BCrypt.Net;
using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using CRM.AlMohtaref.Application.Features.Auth.RefreshToken;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Domain.Extensions;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Auth.ResetPassword
{
    public class ResetPasswordCommandHandler(IUserRepository userRepository, IUserRefreshTokenRepository refreshTokenRepository
        , IPasswordHasher passwordHasherRepository, IUnitOfWork unitOfWork)
        : IRequestHandler<ResetPasswordCommand, bool>
    {
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken ct)
        {
            User user = await userRepository.GetByIdAsync(request.userId, ct) ?? throw new NotFoundException("User not found");

            if(request.newPassword != request.confirmPassword)
            {
                throw new BadRequestException("Passwords do not match");
            }

            user.ChangePassword(passwordHasherRepository.HashPassword(request.newPassword));

            await refreshTokenRepository.RevokeUserTokens(user.Id);

            return await unitOfWork.SaveChangesAsync(ct) > 0;

        }
    }
}
