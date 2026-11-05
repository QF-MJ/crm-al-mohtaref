using MediatR;

namespace CRM.AlMohtaref.Application.Features.Lookups;

public sealed record GetLookupsQuery(List<string> Include) : IRequest<LookupsDto>;