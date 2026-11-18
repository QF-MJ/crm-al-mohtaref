using CRM.AlMohtaref.Application.Features.Auth;
using CRM.AlMohtaref.Application.Features.Auth.Login;
using FluentValidation.Results;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    private static bool HasErrorFor(ValidationResult result, string property) =>
        result.Errors.Any(e => e.PropertyName == property);

    [Fact]
    public void ValidCommand_IsValid() =>
        Assert.True(_validator.Validate(new LoginCommand("john", "secret")).IsValid);

    // ---------- AccountName ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AccountName_EmptyOrWhitespace_IsInvalid(string account) =>
        Assert.True(HasErrorFor(_validator.Validate(new LoginCommand(account, "secret")), "AccountName"));

    [Fact]
    public void AccountName_Null_IsInvalid() =>
        Assert.True(HasErrorFor(_validator.Validate(new LoginCommand(null!, "secret")), "AccountName"));

    [Theory]
    [InlineData(50, true)]
    [InlineData(51, false)]
    public void AccountName_MaxLength50(int length, bool valid) =>
        Assert.Equal(valid, !HasErrorFor(
            _validator.Validate(new LoginCommand(new string('a', length), "secret")), "AccountName"));

    // ---------- Password ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Password_EmptyOrWhitespace_IsInvalid(string password) =>
        Assert.True(HasErrorFor(_validator.Validate(new LoginCommand("john", password)), "Password"));

    [Fact]
    public void Password_Null_IsInvalid() =>
        Assert.True(HasErrorFor(_validator.Validate(new LoginCommand("john", null!)), "Password"));

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Password_MaxLength100(int length, bool valid) =>
        Assert.Equal(valid, !HasErrorFor(
            _validator.Validate(new LoginCommand("john", new string('a', length))), "Password"));

    // ---------- both invalid ----------

    [Fact]
    public void BothInvalid_ReportsBothProperties()
    {
        var result = _validator.Validate(new LoginCommand("", ""));

        Assert.True(HasErrorFor(result, "AccountName"));
        Assert.True(HasErrorFor(result, "Password"));
    }

    // The login validator must not reveal which field was wrong (same message everywhere).
    [Fact]
    public void AllErrorMessages_AreIdentical()
    {
        var result = _validator.Validate(new LoginCommand(new string('a', 51), new string('a', 101)));

        Assert.True(result.Errors.Count >= 2);
        Assert.Single(result.Errors.Select(e => e.ErrorMessage).Distinct());
    }
}