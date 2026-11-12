using CRM.AlMohtaref.Application.Features.Lookups;

namespace CRM.AlMohtaref.Api.Contracts.Lookups;

public sealed record GetLookupsRequest(List<LookupType>? Include)
{
    public GetLookupsQuery ToQuery() => new(Include);
}