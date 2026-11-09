using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Roles = "مدير")]
public sealed record GetUserQuery(int Id) : IRequest<UserDetailDto>;
