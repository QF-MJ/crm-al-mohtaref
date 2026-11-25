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
