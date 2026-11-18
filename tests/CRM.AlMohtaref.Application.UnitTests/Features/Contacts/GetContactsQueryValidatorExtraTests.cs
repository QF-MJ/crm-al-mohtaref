using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Contacts;

public class GetContactsQueryValidatorExtraTests
{
    private readonly GetContactsQueryValidator _validator = new();

    [Fact]
    public void PageNumberZero_IsRejected_ByIncludedPagedValidator() =>
        Assert.True(_validator.Validate(new GetContactsQuery(PageNumber: 0)).HasErrorFor("PageNumber"));

    
    [Fact]
    public void DuplicateTagIds_AreInvalid()
    {
        var id = Guid.NewGuid();

        Assert.True(_validator.Validate(new GetContactsQuery(TagIds: new List<Guid> { id, id })).HasErrorFor("TagIds"));
    }

   
    [Theory]
    [InlineData("   ")]
    public void WhitespaceOnlyFilters_AreInvalid(string value)
    {
        Assert.True(_validator.Validate(new GetContactsQuery(StudentName: value)).HasErrorFor("StudentName"));
        Assert.True(_validator.Validate(new GetContactsQuery(ContactName: value)).HasErrorFor("ContactName"));
        Assert.True(_validator.Validate(new GetContactsQuery(PhoneNumber: value)).HasErrorFor("PhoneNumber"));
    }

  
    [Theory]
    [InlineData("abc")]
    [InlineData("09-99; DROP TABLE")]
    public void NonPhoneCharacters_AreInvalid(string phone) =>
        Assert.True(_validator.Validate(new GetContactsQuery(PhoneNumber: phone)).HasErrorFor("PhoneNumber"));
}
