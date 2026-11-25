using CRM.AlMohtaref.Application.Features.Campaigns;
using FluentValidation.Results;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Campaigns;

public class CreateCampaignCommandValidatorTests
{
    private readonly CreateCampaignCommandValidator _validator = new();

    private static CreateCampaignCommand Make(
        string title = "حملة اختبار",
        string description = "وصف",
        Guid? trackId = null,
        string url = "https://example.com/campaign",
        decimal budget = 100m,
        List<Guid>? platformIds = null) =>
        new(title, description, trackId ?? Guid.NewGuid(), url, budget,
            platformIds ?? new List<Guid> { Guid.NewGuid() });

    private static bool HasErrorFor(ValidationResult result, string property) =>
        result.Errors.Any(e => e.PropertyName == property);

    private bool HasErrorFor(CreateCampaignCommand command, string property) =>
        HasErrorFor(_validator.Validate(command), property);

    [Fact]
    public void ValidCommand_IsValid() =>
        Assert.True(_validator.Validate(Make()).IsValid);

    // ---------- Title ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_EmptyOrWhitespace_IsInvalid(string title) =>
        Assert.True(HasErrorFor(Make(title: title), "Title"));

    [Fact]
    public void Title_Null_IsInvalid() =>
        Assert.True(HasErrorFor(Make(title: null!), "Title"));

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void Title_MaxLength200(int length, bool valid) =>
        Assert.Equal(valid, !HasErrorFor(Make(title: new string('a', length)), "Title"));

    // ---------- Description ----------

    [Fact]
    public void Description_Null_IsValid() =>
        Assert.False(HasErrorFor(Make(description: null!), "Description"));

    [Fact]
    public void Description_Empty_IsValid() =>
        Assert.False(HasErrorFor(Make(description: ""), "Description"));

    [Theory]
    [InlineData(1000, true)]
    [InlineData(1001, false)]
    public void Description_MaxLength1000(int length, bool valid) =>
        Assert.Equal(valid, !HasErrorFor(Make(description: new string('a', length)), "Description"));

    // ---------- TrackId ----------

    [Fact]
    public void TrackId_Empty_IsInvalid() =>
        Assert.True(HasErrorFor(Make(trackId: Guid.Empty), "TrackId"));

    // ---------- Url ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Url_EmptyOrWhitespace_IsInvalid(string url) =>
        Assert.True(HasErrorFor(Make(url: url), "Url"));

    [Fact]
    public void Url_Null_IsInvalid() =>
        Assert.True(HasErrorFor(Make(url: null!), "Url"));

    [Theory]
    [InlineData("not a url")]
    [InlineData("example.com")]
    [InlineData("/relative/path")]
    public void Url_NotAbsolute_IsInvalid(string url) =>
        Assert.True(HasErrorFor(Make(url: url), "Url"));

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/a/b?x=1")]
    public void Url_Absolute_IsValid(string url) =>
        Assert.False(HasErrorFor(Make(url: url), "Url"));

    // ---------- Budget ----------

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(1000000)]
    public void Budget_ZeroOrMore_IsValid(double budget) =>
        Assert.False(HasErrorFor(Make(budget: (decimal)budget), "Budget"));

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Budget_Negative_IsInvalid(double budget) =>
        Assert.True(HasErrorFor(Make(budget: (decimal)budget), "Budget"));

    // ---------- PlatformIds ----------

    [Fact]
    public void PlatformIds_Empty_IsInvalid() =>
        Assert.True(HasErrorFor(Make(platformIds: new List<Guid>()), "PlatformIds"));

    [Fact]
    public void PlatformIds_Null_IsInvalid()
    {
        var command = new CreateCampaignCommand("t", "d", Guid.NewGuid(), "https://example.com", 1m, null!);

        Assert.True(HasErrorFor(command, "PlatformIds"));
    }

    [Fact]
    public void PlatformIds_ContainingEmptyGuid_IsInvalid() =>
        Assert.True(HasErrorFor(
            Make(platformIds: new List<Guid> { Guid.NewGuid(), Guid.Empty }), "PlatformIds[1]"));

    [Fact]
    public void PlatformIds_MultipleValid_IsValid() =>
        Assert.True(_validator.Validate(
            Make(platformIds: new List<Guid> { Guid.NewGuid(), Guid.NewGuid() })).IsValid);

    // ---------- everything invalid ----------

    [Fact]
    public void AllInvalid_ReportsEveryProperty()
    {
        var command = new CreateCampaignCommand(
            "", new string('a', 1001), Guid.Empty, "bad", -1m, new List<Guid>());

        var result = _validator.Validate(command);

        Assert.True(HasErrorFor(result, "Title"));
        Assert.True(HasErrorFor(result, "Description"));
        Assert.True(HasErrorFor(result, "TrackId"));
        Assert.True(HasErrorFor(result, "Url"));
        Assert.True(HasErrorFor(result, "Budget"));
        Assert.True(HasErrorFor(result, "PlatformIds"));
    }
}
