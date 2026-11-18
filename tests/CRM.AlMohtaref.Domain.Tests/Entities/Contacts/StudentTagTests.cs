using CRM.AlMohtaref.Domain.Entities.Contacts;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Contacts;

public class StudentTagTests
{
    [Fact]
    public void NewInstance_HasDefaultIdentifiers()
    {
        var studentTag = new StudentTag();

        Assert.Equal(Guid.Empty, studentTag.Id);
        Assert.Equal(Guid.Empty, studentTag.TagId);
        Assert.Null(studentTag.StudentId);
    }

    [Fact]
    public void NewInstance_HasNoNavigations()
    {
        var studentTag = new StudentTag();

        Assert.Null(studentTag.Student);
    }
}
