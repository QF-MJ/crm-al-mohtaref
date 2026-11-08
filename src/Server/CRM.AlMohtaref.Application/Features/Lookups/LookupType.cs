using System.Text.Json.Serialization;

namespace CRM.AlMohtaref.Application.Features.Lookups;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LookupType
{
    Departments,
    Roles,
    Tracks,
    LeadStatuses
}
