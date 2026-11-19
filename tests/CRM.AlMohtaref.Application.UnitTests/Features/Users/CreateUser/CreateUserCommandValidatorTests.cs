using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Features.Users.CreateUser;
using CRM.AlMohtaref.Domain.Entities.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _sut = new();
    private static CreateUserCommand Valid => UsersTestData.ValidCreate();

    [Fact]
    public void ValidCommand_Passes()
        => _sut.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EverythingEmpty_ReportsAllRequiredFields()
    {
        var cmd = new CreateUserCommand("", "", "", "", null);
        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.AccountName);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    // ------------------------------- Email -------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Email_Empty_Fails(string? email)
        => _sut.TestValidate(Valid with { Email = email! })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@no-local.com")]
    [InlineData("no-domain@")]
    [InlineData("two@@at.com")]
    public void Email_InvalidFormat_Fails(string email)
        => _sut.TestValidate(Valid with { Email = email })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Email_Longer_Than_150_Fails()
        => _sut.TestValidate(Valid with { Email = new string('a', 142) + "@test.com" })
               .ShouldHaveValidationErrorFor(x => x.Email);

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("john.doe+tag@sub.example.co.uk")]
    public void Email_Normal_Passes(string email)
        => _sut.TestValidate(Valid with { Email = email }).ShouldNotHaveValidationErrorFor(x => x.Email);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Email_WithSpaces_ShouldFail()
        // FluentValidation's default (AspNetCoreCompatible) mode only checks the '@' position.
        => _sut.TestValidate(Valid with { Email = "john doe@example.com" })
               .ShouldHaveValidationErrorFor(x => x.Email);

    // ----------------------------- AccountName -----------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AccountName_Empty_Fails(string? name)
        => _sut.TestValidate(Valid with { AccountName = name! }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void AccountName_TooShort_Fails(string name)
        => _sut.TestValidate(Valid with { AccountName = name }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Fact]
    public void AccountName_Longer_Than_50_Fails()
        => _sut.TestValidate(Valid with { AccountName = new string('a', 51) })
               .ShouldHaveValidationErrorFor(x => x.AccountName);

    [Theory]
    [InlineData(3)]
    [InlineData(50)]
    public void AccountName_Boundaries_Pass(int length)
        => _sut.TestValidate(Valid with { AccountName = new string('a', length) })
               .ShouldNotHaveValidationErrorFor(x => x.AccountName);

    [Theory]
    [InlineData("john doe")]
    [InlineData(" john")]
    [InlineData("john ")]
    [InlineData("john@doe")]
    [InlineData("john$")]
    [InlineData("john/doe")]
    [InlineData("john\tdoe")]
    [InlineData("ط¬ظˆظ†_ط¯ظˆ")]
    public void AccountName_InvalidCharacters_Fail(string name)
        => _sut.TestValidate(Valid with { AccountName = name }).ShouldHaveValidationErrorFor(x => x.AccountName);

    [Theory]
    [InlineData("john.doe")]
    [InlineData("John_Doe")]
    [InlineData("john-doe")]
    [InlineData("a.b-c_1")]
    public void AccountName_AllowedCharacters_Pass(string name)
        => _sut.TestValidate(Valid with { AccountName = name }).ShouldNotHaveValidationErrorFor(x => x.AccountName);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void AccountName_WithTrailingNewline_ShouldFail()
        // In .NET, '$' also matches right before a final '\n' -> "john.doe\n" slips through "^...$".
        => _sut.TestValidate(Valid with { AccountName = "john.doe\n" })
               .ShouldHaveValidationErrorFor(x => x.AccountName);

    // ------------------------------ FullName ------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FullName_Empty_Fails(string? name)
        => _sut.TestValidate(Valid with { FullName = name! }).ShouldHaveValidationErrorFor(x => x.FullName);

    [Fact]
    public void FullName_TooShort_Fails()
        => _sut.TestValidate(Valid with { FullName = "ab" }).ShouldHaveValidationErrorFor(x => x.FullName);

    [Fact]
    public void FullName_Longer_Than_100_Fails()
        => _sut.TestValidate(Valid with { FullName = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.FullName);

    [Theory]
    [InlineData(3)]
    [InlineData(100)]
    public void FullName_Boundaries_Pass(int length)
        => _sut.TestValidate(Valid with { FullName = new string('a', length) })
               .ShouldNotHaveValidationErrorFor(x => x.FullName);

    [Fact]
    public void FullName_Arabic_Passes()
        => _sut.TestValidate(Valid with { FullName = "محمد أحمد" }).ShouldNotHaveValidationErrorFor(x => x.FullName);

    // ------------------------------ Password ------------------------------

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Password_Empty_Fails(string? password)
        => _sut.TestValidate(Valid with { Password = password! })
               .ShouldHaveValidationErrorFor(x => x.Password);


    [Fact]
    public void Password_Shorter_Than_8_Fails()
        => _sut.TestValidate(Valid with { Password = "Aa1@aaa" }).ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_Longer_Than_100_Fails()
        => _sut.TestValidate(Valid with { Password = "Aa1@" + new string('a', 97) }) // 101
               .ShouldHaveValidationErrorFor(x => x.Password);

    [Theory]
    [InlineData("Aa1@aaaa")]                        // exactly 8
    [InlineData("Str0ng@Pass")]
    public void Password_Valid_Passes(string password)
        => _sut.TestValidate(Valid with { Password = password }).ShouldNotHaveValidationErrorFor(x => x.Password);

    [Fact]
    public void Password_Exactly_100_Passes()
        => _sut.TestValidate(Valid with { Password = "Aa1@" + new string('a', 96) })
               .ShouldNotHaveValidationErrorFor(x => x.Password);

    [Theory]
    [InlineData("alllower1@")]    // no upper
    [InlineData("ALLUPPER1@")]    // no lower
    [InlineData("NoDigits@@x")]   // no digit
    [InlineData("NoSpecial123")]  // no special
    public void Password_MissingCharacterClass_Fails(string password)
        => _sut.TestValidate(Valid with { Password = password }).ShouldHaveValidationErrorFor(x => x.Password);

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Password_ArabicLetterShouldNotCountAsSpecialCharacter()
        => _sut.TestValidate(Valid with { Password = "Abcdefg1ط¹" }).ShouldHaveValidationErrorFor(x => x.Password);

    // ----------------------------- Permissions -----------------------------

    [Fact]
    public void Permissions_Null_Passes()
        => _sut.TestValidate(Valid with { Permissions = null }).ShouldNotHaveValidationErrorFor(x => x.Permissions);

    [Fact]
    public void Permissions_Empty_Passes()
        => _sut.TestValidate(Valid with { Permissions = new List<PermissionCommand>() })
               .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Permissions_InvalidDepartment_ReportsIndexedProperty()
        => _sut.TestValidate(Valid with { Permissions = UsersTestData.Perms((Role.Employee, Guid.Empty)) })
               .ShouldHaveValidationErrorFor("Permissions[0].DepartmentId");

    [Fact]
    public void Permissions_SecondItemInvalid_ReportsIndexOne()
    {
        var cmd = Valid with
        {
            Permissions = UsersTestData.Perms((Role.Employee, Guid.NewGuid()), ((Role)42, Guid.NewGuid()))
        };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Permissions[1].Role");
        result.ShouldNotHaveValidationErrorFor("Permissions[0].Role");
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Permissions_WithNullElement_ShouldFailValidation()
    {
        // Child validators skip null items, then the handler would NRE on p.Role.
        var cmd = Valid with { Permissions = new List<PermissionCommand> { null! } };

        var result = _sut.TestValidate(cmd);

        Assert.False(result.IsValid);
    }
}