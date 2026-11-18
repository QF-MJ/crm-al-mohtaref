using CRM.AlMohtaref.Domain.Entities.Contacts;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Contacts;

public class ContactTagTests
{
    private static ContactTag Create() =>
        (ContactTag)Activator.CreateInstance(typeof(ContactTag), nonPublic: true)!;

    [Fact]
    public void PrivateConstructor_CanBeInvokedByEfCore()
    {
        var contactTag = Create();

        Assert.NotNull(contactTag);
    }

    [Fact]
    public void NewInstance_HasDefaultIdentifiers()
    {
        var contactTag = Create();

        Assert.Equal(Guid.Empty, contactTag.Id);
        Assert.Equal(Guid.Empty, contactTag.TagId);
        Assert.Null(contactTag.ContactId);
    }

    [Fact]
    public void NewInstance_HasNoContactNavigation()
    {
        var contactTag = Create();

        Assert.Null(contactTag.Contact);
    }

    [Fact]
    public void HasNoPublicConstructors()
    {
        Assert.Empty(typeof(ContactTag).GetConstructors());
    }
}
