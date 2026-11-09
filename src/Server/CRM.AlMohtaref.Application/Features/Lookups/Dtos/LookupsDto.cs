namespace CRM.AlMohtaref.Application.Features.Lookups.Dtos;

public class LookupsDto {

    public IEnumerable<DepartmentDto>? Departments { get; set; }
        
    public IEnumerable<RoleDto>? Roles { get; set; }

    public IEnumerable<LeadStatusDto>? LeadStatuses { get; set; }
        
    public IEnumerable<TrackDto>? Tracks { get; set; }

}