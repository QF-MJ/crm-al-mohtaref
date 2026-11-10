using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Lookups;

public class GetLookupsQueryHandler(ILookupRepository lookup) : IRequestHandler<GetLookupsQuery, LookupsDto>
{
    public async Task<LookupsDto> Handle(GetLookupsQuery request, CancellationToken cancellationToken)
    {
        LookupsDto lookupsDto = new();

        var includes = request.Include.ToHashSet();

        if (includes.Contains(LookupType.Departments))
            lookupsDto.Departments = await lookup.GetDepartmentsAsync(cancellationToken);

        if (includes.Contains(LookupType.Roles))
            lookupsDto.Roles = await lookup.GetRolesAsync(cancellationToken);

        if (includes.Contains(LookupType.Tracks))
            lookupsDto.Tracks = await lookup.GetTracksAsync(cancellationToken);

        if (includes.Contains(LookupType.LeadStatuses))
            lookupsDto.LeadStatuses = await lookup.GetLeadStatusesAsync(cancellationToken);

        return lookupsDto;
    }
}
