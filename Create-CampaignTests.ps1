param(
    [string]$Root = (Get-Location).Path,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$utf8Bom = New-Object System.Text.UTF8Encoding($true)

$validatorDir = Join-Path $Root 'src\Server\CRM.AlMohtaref.Application\Features\Campaigns\CreateCampaign'
$testsDir     = Join-Path $Root 'tests\CRM.AlMohtaref.Application.UnitTests\Features\Campaigns'

if (-not (Test-Path $validatorDir)) { throw "Directory not found: $validatorDir . Run the script from the project root or pass -Root" }
New-Item -ItemType Directory -Force -Path $testsDir | Out-Null

function Write-Source([string]$Path, [string]$Content) {
    if ((Test-Path $Path) -and -not $Force) {
        Write-Host "SKIP (exists): $Path" -ForegroundColor Yellow
        return
    }
    [System.IO.File]::WriteAllText($Path, $Content.TrimStart() + "`r`n", $utf8Bom)
    Write-Host "CREATED: $Path" -ForegroundColor Green
}

# =====================================================================
# 1) CreateCampaignCommandValidator
# =====================================================================
$validator = @'
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Campaigns;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الحملة مطلوب")
            .MaximumLength(200).WithMessage("يجب ألا يتجاوز العنوان 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("يجب ألا يتجاوز الوصف 1000 حرف");

        RuleFor(x => x.TrackId)
            .NotEmpty().WithMessage("المسار مطلوب");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("رابط الحملة مطلوب")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("صيغة الرابط غير صحيحة");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("يجب أن تكون الميزانية صفراً أو أكثر");

        RuleFor(x => x.PlatformIds)
            .NotEmpty().WithMessage("يجب اختيار منصة واحدة على الأقل");

        RuleForEach(x => x.PlatformIds)
            .NotEmpty().WithMessage("معرف المنصة غير صالح");
    }
}
'@
Write-Source (Join-Path $validatorDir 'CreateCampaignCommandValidator.cs') $validator

# =====================================================================
# 2) CreateCampaignCommandValidatorTests
# =====================================================================
$validatorTests = @'
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
'@
Write-Source (Join-Path $testsDir 'CreateCampaignCommandValidatorTests.cs') $validatorTests

# =====================================================================
# 3) CreateCampaignCommandHandlerTests
# =====================================================================
$createHandlerTests = @'
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Campaigns;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Application.Interfaces.Common;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Campaigns;

public class CreateCampaignCommandHandlerTests
{
    private const string Title = "حملة اختبار";

    private static readonly Guid CurrentUserId = Guid.NewGuid();

    private readonly Mock<ICampaignRepository> _campaigns = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private readonly List<string> _calls = new();
    private Campaign? _added;

    private readonly CreateCampaignCommandHandler _handler;

    public CreateCampaignCommandHandlerTests()
    {
        _currentUser.SetupGet(c => c.UserId).Returns(CurrentUserId);

        _campaigns.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

        _campaigns.Setup(r => r.AddAsync(It.IsAny<Campaign>()))
                  .Callback<Campaign>(c =>
                  {
                      _calls.Add("add");
                      _added = c;
                  })
                  .Returns(Task.CompletedTask);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save"))
            .ReturnsAsync(1);

        _handler = new CreateCampaignCommandHandler(_campaigns.Object, _uow.Object, _currentUser.Object);
    }

    private static CreateCampaignCommand Command(string title = Title) =>
        new(title, "وصف الحملة", Guid.NewGuid(), "https://example.com", 500m,
            new List<Guid> { Guid.NewGuid() });

    private Task<Guid> Create(CreateCampaignCommand command, CancellationToken ct = default) =>
        _handler.Handle(command, ct);

    // ---------------------------------------------------------------- duplicate title

    [Fact]
    public async Task DuplicateTitle_ThrowsConflict()
    {
        _campaigns.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), Title, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => Create(Command()));
    }

    [Fact]
    public async Task DuplicateTitle_AddsAndSavesNothing()
    {
        _campaigns.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), Title, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => Create(Command()));

        _campaigns.Verify(r => r.AddAsync(It.IsAny<Campaign>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DuplicateTitle_MessageIsNotEmpty()
    {
        _campaigns.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), Title, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => Create(Command()));

        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
        Assert.Equal(409, ex.StatusCode);
    }

    // ---------------------------------------------------------------- successful creation

    [Fact]
    public async Task ValidCommand_ReturnsIdOfTheAddedCampaign()
    {
        var id = await Create(Command());

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(_added);
        Assert.Equal(_added!.Id, id);
    }

    [Fact]
    public async Task ValidCommand_AddsExactlyOneCampaign()
    {
        await Create(Command());

        _campaigns.Verify(r => r.AddAsync(It.IsAny<Campaign>()), Times.Once);
    }

    [Fact]
    public async Task ValidCommand_SavesExactlyOnce()
    {
        await Create(Command());

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidCommand_SaveRunsAfterAdd()
    {
        await Create(Command());

        Assert.Equal(new[] { "add", "save" }, _calls);
    }

    [Fact]
    public async Task ValidCommand_ChecksTitleUniquenessBeforeAdding()
    {
        await Create(Command());

        _campaigns.Verify(
            r => r.ExistsAsync(It.IsAny<Guid>(), Title, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TwoCalls_ReturnDifferentIds()
    {
        var first = await Create(Command("حملة 1"));
        var second = await Create(Command("حملة 2"));

        Assert.NotEqual(first, second);
    }

    // ---------------------------------------------------------------- cancellation

    [Fact]
    public async Task CancellationToken_IsPassedToExistsCheck()
    {
        using var cts = new CancellationTokenSource();

        await Create(Command(), cts.Token);

        _campaigns.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), Title, cts.Token), Times.Once);
    }
}
'@
Write-Source (Join-Path $testsDir 'CreateCampaignCommandHandlerTests.cs') $createHandlerTests

# =====================================================================
# 4) GetCampaignQueryHandlerTests
# =====================================================================
$getHandlerTests = @'
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaign;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Campaigns;

public class GetCampaignQueryHandlerTests
{
    private readonly Mock<ICampaignRepository> _repo = new();
    private readonly GetCampaignQueryHandler _handler;

    public GetCampaignQueryHandlerTests()
    {
        _handler = new GetCampaignQueryHandler(_repo.Object);
    }

    private static CampaignDto MakeDto(Guid id) =>
        new(id, "حملة", "وصف", "https://example.com", 100m, "مسار", "مستخدم", "نشطة",
            new List<string> { "Facebook", "Instagram" });

    [Fact]
    public async Task ExistingCampaign_ReturnsTheDto()
    {
        var id = Guid.NewGuid();
        var dto = MakeDto(id);
        _repo.Setup(r => r.GetDtoByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _handler.Handle(new GetCampaignQuery(id), CancellationToken.None);

        Assert.Same(dto, result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task UnknownCampaign_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetDtoByIdAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync((CampaignDto?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetCampaignQuery(id), CancellationToken.None));

        Assert.Equal(404, ex.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    [Fact]
    public async Task EmptyGuid_ThrowsNotFound_WhenRepositoryReturnsNull()
    {
        _repo.Setup(r => r.GetDtoByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
             .ReturnsAsync((CampaignDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetCampaignQuery(Guid.Empty), CancellationToken.None));
    }

    [Fact]
    public async Task Handler_QueriesRepositoryOnce_WithTheRequestedId()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetDtoByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(MakeDto(id));

        await _handler.Handle(new GetCampaignQuery(id), CancellationToken.None);

        _repo.Verify(r => r.GetDtoByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CancellationToken_IsPassedToRepository()
    {
        using var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetDtoByIdAsync(id, cts.Token)).ReturnsAsync(MakeDto(id));

        await _handler.Handle(new GetCampaignQuery(id), cts.Token);

        _repo.Verify(r => r.GetDtoByIdAsync(id, cts.Token), Times.Once);
    }
}
'@
Write-Source (Join-Path $testsDir 'GetCampaignQueryHandlerTests.cs') $getHandlerTests

# =====================================================================
# 5) GetCampaignsPageQueryHandlerTests
# =====================================================================
$pageHandlerTests = @'
using CRM.AlMohtaref.Application.Features.Campaigns.Dtos;
using CRM.AlMohtaref.Application.Features.Campaigns.GetCampaignsPage;
using CRM.AlMohtaref.Application.Interfaces;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Campaigns;

public class GetCampaignsPageQueryHandlerTests
{
    private readonly Mock<ICampaignRepository> _repo = new();
    private readonly GetCampaignsPageQueryHandler _handler;

    public GetCampaignsPageQueryHandlerTests()
    {
        _handler = new GetCampaignsPageQueryHandler(_repo.Object);
    }

    private static CampaignListItemDto Item(string title) =>
        new(Guid.NewGuid(), title, 100m, "مسار", "نشطة");

    [Fact]
    public async Task ReturnsWhatTheRepositoryReturns()
    {
        var query = new GetCampaignsPageQuery();
        var items = new List<CampaignListItemDto> { Item("أ"), Item("ب") };
        _repo.Setup(r => r.GetAll(query)).ReturnsAsync(items);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Same(items, result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task NoCampaigns_ReturnsEmptyList()
    {
        var query = new GetCampaignsPageQuery();
        _repo.Setup(r => r.GetAll(query)).ReturnsAsync(new List<CampaignListItemDto>());

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task PassesTheSameQueryToRepository_Once()
    {
        var query = new GetCampaignsPageQuery(
            PageNumber: 3, Title: "حملة", Budget: 500m, TrackName: "مسار", CampaignStatus: "نشطة");
        _repo.Setup(r => r.GetAll(It.IsAny<GetCampaignsPageQuery>()))
             .ReturnsAsync(new List<CampaignListItemDto>());

        await _handler.Handle(query, CancellationToken.None);

        _repo.Verify(r => r.GetAll(query), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RepositoryException_IsPropagated()
    {
        var query = new GetCampaignsPageQuery();
        _repo.Setup(r => r.GetAll(query)).ThrowsAsync(new InvalidOperationException("db"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public void DefaultQuery_StartsAtPageOne_WithNoFilters()
    {
        var query = new GetCampaignsPageQuery();

        Assert.Equal(1, query.PageNumber);
        Assert.Null(query.Title);
        Assert.Null(query.Budget);
        Assert.Null(query.TrackName);
        Assert.Null(query.CampaignStatus);
    }
}
'@
Write-Source (Join-Path $testsDir 'GetCampaignsPageQueryHandlerTests.cs') $pageHandlerTests

Write-Host "`nDone. Run: dotnet test tests\CRM.AlMohtaref.Application.UnitTests" -ForegroundColor Cyan
