using CRM.AlMohtaref.Application.Features.Lookups;
using CRM.AlMohtaref.Application.Features.Lookups.GetLookups;

namespace CRM.AlMohtaref.Api.Contracts.Lookups;

public sealed record GetLookupsRequest(List<LookupType>? Include)
{
    public GetLookupsQuery ToQuery() => new(Include);
}