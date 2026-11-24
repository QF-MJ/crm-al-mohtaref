using CRM.AlMohtaref.Domain.Entities.Contacts;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Contacts;

public class TagTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var tag = new Tag("VIP");

        Assert.Equal("VIP", tag.Name);
    }

    [Fact]
    public void Constructor_DoesNotAssignId()
    {
        var tag = new Tag("VIP");

        Assert.Equal(Guid.Empty, tag.Id);
    }

    [Fact]
    public void Constructor_InitializesContactTagsAsEmpty()
    {
        var tag = new Tag("VIP");

        Assert.NotNull(tag.ContactTags);
        Assert.Empty(tag.ContactTags);
    }

    [Fact]
    public void Constructor_InitializesOptionTagsAsEmpty()
    {
        var tag = new Tag("VIP");

        Assert.NotNull(tag.OptionTags);
        Assert.Empty(tag.OptionTags);
    }

    [Fact]
    public void Constructor_InitializesStudentTagsAsEmpty()
    {
        var tag = new Tag("VIP");

        Assert.NotNull(tag.StudentTags);
        Assert.Empty(tag.StudentTags);
    }

    [Fact]
    public void PrivateConstructor_SetsEmptyName()
    {
        var tag = (Tag)Activator.CreateInstance(typeof(Tag), nonPublic: true)!;

        Assert.Equal(string.Empty, tag.Name);
    }
}
