using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

namespace CRM.AlMohtaref.Application.Features.Lookups
{
    public class GetLookupsQueryHandler : IRequestHandler<GetLookupsQuery, LookupsDto>
    {
        public readonly ILookupRepository _lookup;

        public GetLookupsQueryHandler(ILookupRepository lookup)
        {
            _lookup = lookup;
        }

        public async Task<LookupsDto> Handle(GetLookupsQuery request, CancellationToken cancellationToken)
        {
            LookupsDto lookupsDto = new LookupsDto();

            if (request.Include.Contains("Departments", StringComparer.OrdinalIgnoreCase))
                lookupsDto.Departments = await _lookup.GetDepartmentsAsync(cancellationToken);

            if (request.Include.Contains("Roles", StringComparer.OrdinalIgnoreCase))
                lookupsDto.Roles = await _lookup.GetRolesAsync(cancellationToken);

            return lookupsDto;
        }
    }
}
