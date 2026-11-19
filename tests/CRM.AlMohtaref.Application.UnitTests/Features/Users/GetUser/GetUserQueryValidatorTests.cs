using CRM.AlMohtaref.Application.Features.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class GetUserQueryValidatorTests
{
    private readonly GetUserQueryValidator _sut = new();

    [Fact]
    public void ValidId_Passes()
        => _sut.TestValidate(new GetUserQuery(Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EmptyId_Fails()
        => _sut.TestValidate(new GetUserQuery(Guid.Empty))
               .ShouldHaveValidationErrorFor(x => x.Id);
}