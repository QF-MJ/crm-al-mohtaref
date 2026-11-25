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
