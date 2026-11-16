namespace CRM.AlMohtaref.Application.Features.Lookups.Dtos;

public class LookupsDto {

    public IEnumerable<DepartmentDto>? Departments { get; set; }
        
    public IEnumerable<TagDto>? Tags { get; set; }
        
    public IEnumerable<TrackDto>? Tracks { get; set; }

}