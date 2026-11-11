using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Lookups;

public sealed record GetLookupsQuery(List<LookupType>? Include) : IRequest<LookupsDto>;