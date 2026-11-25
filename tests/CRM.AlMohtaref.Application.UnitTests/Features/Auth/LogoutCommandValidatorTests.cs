using CRM.AlMohtaref.Application.Features.Auth.Logout;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Auth;

public class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    private static string Id => Guid.NewGuid().ToString();

    [Fact]
    public void ValidCommand_IsValid() =>
        Assert.True(_validator.Validate(new LogoutCommand($"{Id}.secret")).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyOrWhitespace_IsInvalid(string token) =>
        Assert.False(_validator.Validate(new LogoutCommand(token)).IsValid);

    [Fact]
    public void Null_IsInvalid() =>
        Assert.False(_validator.Validate(new LogoutCommand(null!)).IsValid);

    [Theory]
    [InlineData("no-separator")]
    [InlineData("not-a-guid.secret")]
    [InlineData("a.b.c")]
    public void WrongFormat_IsInvalid(string token) =>
        Assert.False(_validator.Validate(new LogoutCommand(token)).IsValid);

    [Fact]
    public void MissingSecretPart_IsInvalid() =>
        Assert.False(_validator.Validate(new LogoutCommand($"{Id}.")).IsValid);

    [Fact]
    public void ThreeParts_IsInvalid() =>
        Assert.False(_validator.Validate(new LogoutCommand($"{Id}.abc.def")).IsValid);

    [Theory]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void MaxLength500(int length, bool valid)
    {
        var prefix = $"{Id}.";
        var token = prefix + new string('a', length - prefix.Length);

        Assert.Equal(valid, _validator.Validate(new LogoutCommand(token)).IsValid);
    }

    // The validator must not reveal why the token was rejected (same message everywhere).
    [Fact]
    public void AllErrorMessages_AreIdentical()
    {
        var result = _validator.Validate(new LogoutCommand(""));

        Assert.NotEmpty(result.Errors);
        Assert.Single(result.Errors.Select(e => e.ErrorMessage).Distinct());
    }
}