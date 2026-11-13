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
        var person = new Person(Guid.NewGuid(), request.FullName, currentUser.UserId);
       
        var user = new User(Guid.NewGuid(), request.AccountName, password.HashPassword(request.Password),
            person, request.Email);

        user.SynchronizePermissions
        (
            request.Permissions?.Select(p => (p.RoleId, p.DepartmentId)) ?? new List<(Guid, Guid)>()
        );

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync();

        return user.Id;
    }
}
