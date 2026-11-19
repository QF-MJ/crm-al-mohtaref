using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _sut = new();
    private static UpdateUserCommand Valid => UsersTestData.ValidUpdate();

    [Fact]
    public void ValidCommand_Passes()
        => _sut.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IsActive_Null_Passes()
        => _sut.TestValidate(Valid with { IsActive = null }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EmptyId_Fails()
        => _sut.TestValidate(Valid with { Id = Guid.Empty })
               .ShouldHaveValidationErrorFor(x => x.Id);

    // AccountName
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AccountName_Empty_Fails(string? value)
        => _sut.TestValidate(Valid with { AccountName = value! }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Theory]
    [InlineData(100, false)]
    [InlineData(101, true)]
    public void AccountName_MaxLength(int length, bool shouldFail)
    {
        var result = _sut.TestValidate(Valid with { AccountName = new string('a', length) });
        if (shouldFail) result.ShouldHaveValidationErrorFor(x => x.AccountName);
        else result.ShouldNotHaveValidationErrorFor(x => x.AccountName);
    }

    // FullName
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void FullName_Empty_Fails(string? value)
        => _sut.TestValidate(Valid with { FullName = value! }).ShouldHaveValidationErrorFor(x => x.FullName);

    [Theory]
    [InlineData(200, false)]
    [InlineData(201, true)]
    public void FullName_MaxLength(int length, bool shouldFail)
    {
        var result = _sut.TestValidate(Valid with { FullName = new string('a', length) });
        if (shouldFail) result.ShouldHaveValidationErrorFor(x => x.FullName);
        else result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    // Email
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Email_Empty_Fails(string? value)
        => _sut.TestValidate(Valid with { Email = value! }).ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("plain")]
    [InlineData("@x.com")]
    [InlineData("x@")]
    [InlineData("a@@b.com")]
    public void Email_InvalidFormat_Fails(string value)
        => _sut.TestValidate(Valid with { Email = value }).ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Email_Longer_Than_256_Fails()
        => _sut.TestValidate(Valid with { Email = new string('a', 248) + "@test.com" }) // 257
               .ShouldHaveValidationErrorFor(x => x.Email);

    // Permissions
    [Fact]
    public void Permissions_Null_Fails()
        => _sut.TestValidate(Valid with { Permissions = null }).ShouldHaveValidationErrorFor(x => x.Permissions);

    [Fact]
    public void Permissions_Empty_Fails()
        => _sut.TestValidate(Valid with { Permissions = new List<PermissionCommand>() })
               .ShouldHaveValidationErrorFor(x => x.Permissions);

    [Fact]
    public void Permissions_InvalidItem_ReportsIndexedProperty()
        => _sut.TestValidate(Valid with { Permissions = UsersTestData.Perms((Role.Employee, Guid.Empty)) })
               .ShouldHaveValidationErrorFor("Permissions[0].DepartmentId");

    [Fact]
    public void Permissions_InvalidRole_ReportsIndexedProperty()
        => _sut.TestValidate(Valid with { Permissions = UsersTestData.Perms(((Role)0, Guid.NewGuid())) })
               .ShouldHaveValidationErrorFor("Permissions[0].Role");

    // ------------------------- BUG PROBES (expected to FAIL on current code) -------------------------
    // Update rules are looser than Create rules for the very same fields.

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData("john doe")]
    [InlineData("john@doe")]
    [InlineData("جون")]
    [InlineData("ab")]
    public void AccountName_ShouldFollowTheSameRulesAsCreate(string value)
        => _sut.TestValidate(Valid with { AccountName = value }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void AccountName_Longer_Than_50_ShouldFail_LikeCreate()
        => _sut.TestValidate(Valid with { AccountName = new string('a', 51) }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void AccountName_WithTrailingNewline_ShouldFail()
        => _sut.TestValidate(Valid with { AccountName = "john.doe\n" }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void FullName_TooShort_ShouldFail_LikeCreate()
        => _sut.TestValidate(Valid with { FullName = "ab" }).ShouldHaveValidationErrorFor(x => x.FullName);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void FullName_Longer_Than_100_ShouldFail_LikeCreate()
        => _sut.TestValidate(Valid with { FullName = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.FullName);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Email_Longer_Than_150_ShouldFail_LikeCreate()
        => _sut.TestValidate(Valid with { Email = new string('a', 142) + "@test.com" }).ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Permissions_WithNullElement_ShouldFailValidation()
    {
        var result = _sut.TestValidate(Valid with { Permissions = new List<PermissionCommand> { null! } });

        Assert.False(result.IsValid);
    }
}
