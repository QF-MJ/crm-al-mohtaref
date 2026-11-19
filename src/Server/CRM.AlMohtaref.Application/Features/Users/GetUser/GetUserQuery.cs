using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;


public sealed record GetUserQuery(Guid Id) : IRequest<UserDetailDto>;
