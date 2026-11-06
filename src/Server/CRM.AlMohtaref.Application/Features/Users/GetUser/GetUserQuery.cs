using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

public sealed record GetUserQuery(int Id) : IRequest<UserDetailDto>;
