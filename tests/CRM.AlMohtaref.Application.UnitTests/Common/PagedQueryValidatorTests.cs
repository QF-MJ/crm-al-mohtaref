using CRM.AlMohtaref.Application.Common.Queries;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Common;

public class PagedQueryValidatorTests
{
    private readonly PagedQueryValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void PageNumber_AtLeastOne_IsValid(int page)
    {
        Assert.True(_validator.Validate(new PagedQuery(page)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void PageNumber_LessThanOne_IsInvalid(int page)
    {
        var result = _validator.Validate(new PagedQuery(page));

        Assert.True(result.HasErrorFor(nameof(PagedQuery.PageNumber)));
    }
}
