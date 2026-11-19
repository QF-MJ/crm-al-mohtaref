using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UpdateUserCommandHandler(IUserRepository userRepository, IUserRefreshTokenRepository userRefreshToken, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.Id, ct);

        if (user == null)
            throw new NotFoundException($"المستخدم بالمعرف {request.Id} غير موجود ");

        if (await userRepository.ExistsAsync(request.Id, request.Email, request.AccountName, ct))
            throw new ConflictException("البريد الإلكتروني أو اسم الحساب مستخدم مسبقاً");

        user.UpdateProfile
        (
            request.AccountName,
            request.Email,
            request.IsActive,
            request.FullName
        );

        var permissions = request.Permissions?.Select(p => (p.Role, p.DepartmentId));
        bool permissionsChanged = user.HasPermissionsChanged(permissions ?? []);

        if(permissionsChanged)
            user.SynchronizePermissions(permissions ?? []);

        if (user.IsActive == false || permissionsChanged)
        {
            await userRefreshToken.RevokeUserTokens(user.Id, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}

