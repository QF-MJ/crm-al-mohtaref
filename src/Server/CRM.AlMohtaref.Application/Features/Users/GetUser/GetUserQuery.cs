using CRM.AlMohtaref.Application.Common.Attributes;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Users;

[AuthorizePermisions(Role = "مدير" , Department = "مبيعات")]
public sealed record GetUserQuery(int Id) : IRequest<UserDetailDto>;
