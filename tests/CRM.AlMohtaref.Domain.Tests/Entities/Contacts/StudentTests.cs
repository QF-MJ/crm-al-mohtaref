using CRM.AlMohtaref.Domain.Entities.Contacts;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Contacts;

public class StudentTests
{
    [Fact]
    public void Constructor_SetsPersonIdAndContactId()
    {
        var contactId = Guid.NewGuid();

        var student = new Student(contactId);

        Assert.Equal(contactId, student.ContactId);
    }

    [Fact]
    public void Constructor_InitializesCollectionsAsEmpty()
    {
        var student = new Student(Guid.NewGuid());

        Assert.NotNull(student.CampaignLeads);
        Assert.Empty(student.CampaignLeads);
        Assert.NotNull(student.StudentTags);
        Assert.Empty(student.StudentTags);
    }

    [Fact]
    public void Constructor_DoesNotAssignId()
    {
        var student = new Student(Guid.NewGuid());

        Assert.Equal(Guid.Empty, student.Id);
    }

    [Fact]
    public void Constructor_LeavesContactNavigationUnloaded()
    {
        var student = new Student(Guid.NewGuid());

        // Navigation property is populated by EF Core, not by the constructor.
        Assert.Null(student.Contact);
    }
}
