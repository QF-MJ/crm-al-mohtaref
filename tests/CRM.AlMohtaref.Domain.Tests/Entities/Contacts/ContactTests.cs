using CRM.AlMohtaref.Domain.Entities.Contacts;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Contacts;

public class ContactTests
{
    [Fact]
    public void Constructor_SetsPersonIdAndPhoneNumber()
    {
        var contact = new Contact("0999999999");

        Assert.Equal("0999999999", contact.PhoneNumber);
    }

    [Fact]
    public void Constructor_InitializesCollectionsAsEmpty()
    {
        var contact = new Contact("0999999999");

        Assert.NotNull(contact.Students);
        Assert.Empty(contact.Students);
        Assert.NotNull(contact.ContactTags);
        Assert.Empty(contact.ContactTags);
    }

    [Fact]
    public void Constructor_DoesNotAssignId()
    {
        var contact = new Contact("0999999999");

        Assert.Equal(Guid.Empty, contact.Id);
    }

    [Fact]
    public void Students_CanBeModified()
    {
        var contact = new Contact("0999999999");
        var student = new Student(contact.Id);

        contact.Students.Add(student);

        Assert.Single(contact.Students);
        Assert.Contains(student, contact.Students);
    }

    [Fact]
    public void Constructor_AllowsPrivateParameterlessCtorViaReflection_WithDefaults()
    {
        var contact = (Contact)Activator.CreateInstance(typeof(Contact), nonPublic: true)!;

        Assert.Equal(string.Empty, contact.PhoneNumber);
        Assert.Empty(contact.Students);
        Assert.Empty(contact.ContactTags);
    }
}
