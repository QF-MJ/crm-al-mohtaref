using CRM.AlMohtaref.Domain.Entities.Campaigns;

namespace CRM.AlMohtaref.Application.Features.Contacts;

/// <summary>GET /api/contacts/{id} — جهة التواصل هي نقطة التجميع، وكل طالب يحمل أقسامه منفصلة.</summary>
public class ContactDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<ContactDetailStudentDto> Students { get; set; } = new();
}

public class ContactDetailStudentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ContactProfileSectionDto> Sections { get; set; } = new();
}

public class ContactProfileSectionDto
{
    /// <summary>opportunities | communications | subscriptions | guidance | support</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>loaded | empty | unavailable</summary>
    public string State { get; set; } = "empty";

    public List<ContactProfileRowDto> Rows { get; set; } = new();
}

public class ContactProfileRowDto
{
    public string? Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Meta { get; set; }
}

/// <summary>صف وسيط من CampaignLead قبل تحويله إلى صف قسم فرص التسجيل.</summary>
public class ContactLeadRowDto
{
    public string CampaignTitle { get; set; } = string.Empty;
    public string TrackName { get; set; } = string.Empty;
    public LeadStatus Status { get; set; }
}
