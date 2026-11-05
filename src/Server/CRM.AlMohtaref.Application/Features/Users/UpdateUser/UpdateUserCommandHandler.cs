using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.Id, ct);

        if (user == null)
            throw new NotFoundException($"المستخدم بالمعرف {request.Id} غير موجود ");

        if (await _users.ExistsAsync(request.Id, request.Email, request.AccountName,  ct))
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
            request.Permissions?.Select(p => (p.RoleId, p.DepartmentId)) ?? new List<(int, int)>()
        );


        await _unitOfWork.SaveChangesAsync(ct);
    }
}

