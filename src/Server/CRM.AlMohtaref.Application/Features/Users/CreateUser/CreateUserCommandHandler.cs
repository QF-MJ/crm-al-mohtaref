using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public class CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, 
    ICurrentUserService currentUser, IPasswordHasher password)
    : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {

        if (await userRepository.ExistsAsync(Guid.Empty, request.Email, request.AccountName, cancellationToken))
            throw new ConflictException("البريد الإلكتروني أو اسم الحساب مستخدم مسبقاً");

        var person = new Person(request.FullName, currentUser.UserId);
       
        var user = new User(Guid.NewGuid(), request.AccountName, password.HashPassword(request.Password),
            person, request.Email);

        user.SynchronizePermissions
        (
            request.Permissions?.Select(p => (p.Role, p.DepartmentId)) ?? new List<(Role, Guid)>()
        );

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
