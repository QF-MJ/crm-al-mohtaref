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
