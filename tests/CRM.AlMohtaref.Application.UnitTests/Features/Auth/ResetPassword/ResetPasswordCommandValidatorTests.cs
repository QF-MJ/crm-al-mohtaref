using CRM.AlMohtaref.Application.Features.Auth.ResetPassword;
using FluentValidation.Results;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth.ResetPassword;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    private static ResetPasswordCommand Make(
        Guid? userId = null,
        string newPassword = "Passw0rd!",
        string? confirmPassword = null) =>
        new(userId ?? Guid.NewGuid(), newPassword, confirmPassword ?? newPassword);

    private static bool HasErrorFor(ValidationResult result, string property) =>
        result.Errors.Any(e => e.PropertyName == property);

    private bool HasErrorFor(ResetPasswordCommand command, string property) =>
        HasErrorFor(_validator.Validate(command), property);

    [Fact]
    public void ValidCommand_IsValid() =>
        Assert.True(_validator.Validate(Make()).IsValid);

    // ---------- userId ----------

    [Fact]
    public void UserId_Empty_IsInvalid() =>
        Assert.True(HasErrorFor(Make(userId: Guid.Empty), "userId"));

    [Fact]
    public void UserId_NonEmpty_IsValid() =>
        Assert.False(HasErrorFor(Make(userId: Guid.NewGuid()), "userId"));

    // ---------- newPassword ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NewPassword_EmptyOrWhitespace_IsInvalid(string password) =>
        Assert.True(HasErrorFor(Make(newPassword: password), "newPassword"));

    [Fact]
    public void NewPassword_Null_IsInvalid() =>
        Assert.True(HasErrorFor(Make(newPassword: null!, confirmPassword: "x"), "newPassword"));

    [Theory]
    [InlineData(7, false)]
    [InlineData(8, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void NewPassword_LengthBetween8And100(int length, bool valid) =>
        Assert.Equal(valid, !HasErrorFor(Make(newPassword: new string('a', length)), "newPassword"));

    // ---------- confirmPassword ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfirmPassword_EmptyOrWhitespace_IsInvalid(string confirm) =>
        Assert.True(HasErrorFor(Make(confirmPassword: confirm), "confirmPassword"));

    [Fact]
    public void ConfirmPassword_Null_IsInvalid() =>
        Assert.True(HasErrorFor(
            new ResetPasswordCommand(Guid.NewGuid(), "Passw0rd!", null!), "confirmPassword"));

    [Fact]
    public void ConfirmPassword_Different_IsInvalid() =>
        Assert.True(HasErrorFor(
            Make(newPassword: "Passw0rd!", confirmPassword: "Different1!"), "confirmPassword"));

    [Fact]
    public void ConfirmPassword_DifferentOnlyByCase_IsInvalid() =>
        Assert.True(HasErrorFor(
            Make(newPassword: "Passw0rd!", confirmPassword: "passw0rd!"), "confirmPassword"));

    [Fact]
    public void ConfirmPassword_Same_IsValid() =>
        Assert.False(HasErrorFor(Make(newPassword: "Passw0rd!", confirmPassword: "Passw0rd!"), "confirmPassword"));

    // ---------- everything invalid ----------

    [Fact]
    public void AllInvalid_ReportsEveryProperty()
    {
        var result = _validator.Validate(new ResetPasswordCommand(Guid.Empty, "", ""));

        Assert.True(HasErrorFor(result, "userId"));
        Assert.True(HasErrorFor(result, "newPassword"));
        Assert.True(HasErrorFor(result, "confirmPassword"));
    }

    [Fact]
    public void ErrorMessages_AreNotEmpty()
    {
        var result = _validator.Validate(new ResetPasswordCommand(Guid.Empty, "", "x"));

        Assert.NotEmpty(result.Errors);
        Assert.All(result.Errors, e => Assert.False(string.IsNullOrWhiteSpace(e.ErrorMessage)));
    }
}
