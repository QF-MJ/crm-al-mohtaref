using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Enums;

namespace CRM.AlMohtaref.Domain.UnitTests.Entities.Campaigns;

public class CampaignTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var title = "حملة التسويق الصيفية";
        var trackId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var status = CampaignStatus.New;
        var description = "وصف الحملة";
        var url = "https://example.com";
        var budget = 1500.50m;

        // Act
        var campaign = new Campaign(title, trackId, userId, status, description, url, budget);

        // Assert
        Assert.Equal(title, campaign.Title);
        Assert.Equal(trackId, campaign.TrackId);
        Assert.Equal(userId, campaign.CreatedByUserId);
        Assert.Equal(status, campaign.CampaignStatus);
        Assert.Equal(description, campaign.Description);
        Assert.Equal(url, campaign.Url);
        Assert.Equal(budget, campaign.Budget);
        Assert.NotNull(campaign.Leads);
        Assert.Empty(campaign.Leads);
        Assert.NotNull(campaign.PlatformCampaigns);
        Assert.Empty(campaign.PlatformCampaigns);
        Assert.NotNull(campaign.QuestionCampaigns);
        Assert.Empty(campaign.QuestionCampaigns);
    }

    [Fact]
    public void ChangeStatus_WhenCalled_ShouldUpdateCampaginStatusId()
    {
        // Arrange
        var initialStatusId = CampaignStatus.New;
        var newStatusId = Enums.CampaignStatus.Cancelled;
        var campaign = new Campaign("عنوان", Guid.NewGuid(), Guid.NewGuid(), initialStatusId);

        // Act
        campaign.ChangeStatus(newStatusId);

        // Assert
        Assert.Equal(newStatusId, campaign.CampaignStatus);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_ShouldUpdateAllDetails()
    {
        // Arrange
        var campaign = new Campaign("عنوان قديم", Guid.NewGuid(), Guid.NewGuid(), CampaignStatus.New);
        var newTitle = "عنوان جديد";
        var newDescription = "وصف جديد";
        var newStatusId = CampaignStatus.NeedsModification;
        var newTrackId = Guid.NewGuid();
        var newUrl = "https://new-url.com";
        var newBudget = 5000m;

        // Act
        campaign.UpdateDetails(newTitle, newDescription, newStatusId, newTrackId, newUrl, newBudget);

        // Assert
        Assert.Equal(newTitle, campaign.Title);
        Assert.Equal(newDescription, campaign.Description);
        Assert.Equal(newStatusId, campaign.CampaignStatus);
        Assert.Equal(newTrackId, campaign.TrackId);
        Assert.Equal(newUrl, campaign.Url);
        Assert.Equal(newBudget, campaign.Budget);
    }

    [Fact]
    public void SynchronizePlatforms_WhenAddingNewPlatforms_ShouldAddDistinctPlatforms()
    {
        // Arrange
        var campaign = new Campaign("حملة", Guid.NewGuid(), Guid.NewGuid(), CampaignStatus.New);
        var platform1 = Guid.NewGuid();
        var platform2 = Guid.NewGuid();
        // تمرير معرف مكرر للتأكد من عمل Distinct
        var platformIds = new List<Guid> { platform1, platform2, platform1 };

        // Act
        campaign.SynchronizePlatforms(platformIds);

        // Assert
        Assert.Equal(2, campaign.PlatformCampaigns.Count);
        Assert.Contains(campaign.PlatformCampaigns, p => p.PlatformId == platform1);
        Assert.Contains(campaign.PlatformCampaigns, p => p.PlatformId == platform2);
    }

    [Fact]
    public void SynchronizePlatforms_WhenUpdatingExistingPlatforms_ShouldAddNewAndRemoveOld()
    {
        // Arrange
        var campaign = new Campaign("حملة", Guid.NewGuid(), Guid.NewGuid(), CampaignStatus.New);
        var platformToKeep = Guid.NewGuid();
        var platformToRemove = Guid.NewGuid();
        var platformToAdd = Guid.NewGuid();

        campaign.SynchronizePlatforms(new[] { platformToKeep, platformToRemove });

        // Act
        campaign.SynchronizePlatforms(new[] { platformToKeep, platformToAdd });

        // Assert
        Assert.Equal(2, campaign.PlatformCampaigns.Count);
        Assert.Contains(campaign.PlatformCampaigns, p => p.PlatformId == platformToKeep);
        Assert.Contains(campaign.PlatformCampaigns, p => p.PlatformId == platformToAdd);
        Assert.DoesNotContain(campaign.PlatformCampaigns, p => p.PlatformId == platformToRemove);
    }
}
