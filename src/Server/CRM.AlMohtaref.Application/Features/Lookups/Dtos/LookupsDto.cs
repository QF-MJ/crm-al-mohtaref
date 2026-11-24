using System.Text.Json.Serialization;

namespace CRM.AlMohtaref.Application.Features.Lookups.Dtos;

public class LookupsDto {

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<LookupItemDto>? Departments { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<LookupItemDto>? Tags { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<LookupItemDto>? Tracks { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<LookupItemDto>? Campaigns { get; set; }

}