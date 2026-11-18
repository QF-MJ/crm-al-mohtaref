namespace CRM.AlMohtaref.Application.Features.Lookups.Dtos;

public class LookupsDto {

    public IEnumerable<LookupItemDto>? Departments { get; set; }
        
    public IEnumerable<LookupItemDto>? Tags { get; set; }
        
    public IEnumerable<LookupItemDto>? Tracks { get; set; }

    public IEnumerable<LookupItemDto>? Campaigns { get; set; }

}