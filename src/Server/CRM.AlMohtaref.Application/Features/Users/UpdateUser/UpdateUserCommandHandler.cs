using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
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

        user.SynchronizePermissions
        (
            request.Permissions?.Select(p => (p.RoleId, p.DepartmentId)) ?? new List<(Guid, Guid)>()
        );


        await unitOfWork.SaveChangesAsync(ct);
    }
}

