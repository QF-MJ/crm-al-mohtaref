using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Lookups.GetUsers;

public sealed class GetUsersCommandHandler(ILookupRepository lookupRepository, ICurrentUserService currentUser)
    : IOwnershipChecker<GetUsersCommand>, IRequestHandler<GetUsersCommand, IEnumerable<LookupItemDto>>
{
    public async Task<IEnumerable<LookupItemDto>> Handle(GetUsersCommand request, CancellationToken cancellationToken)
    {
        if (!await IsOwnerAsync(request))
            throw new ForbiddenException("لا يمكنك عرض المستخدمين التابعين لمدير آخر");

        return await lookupRepository.GetUsersByManagerIdAsync(request.ManagerId, cancellationToken);
    }

    public Task<bool> IsOwnerAsync(GetUsersCommand request, CancellationToken ct = default)
    {
        return Task.FromResult(request.ManagerId == currentUser.UserId);
    }
}
