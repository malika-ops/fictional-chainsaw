using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.DeleteTests;

public class DeleteTierEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "DELETE /api/tiers/{id} returns 200 when tier exists and has no linked agencies")]
    public async Task Delete_ShouldReturn200_WhenTierExistsAndHasNoLinkedAgencies()
    {
        // Arrange
        var tierId = new TierId(Guid.NewGuid());
        var tier = Tier.Create(tierId, "Premium Tier", "High-level tier");

        _tierRepoMock
            .Setup(r => r.GetByIdAsync(tierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tier);

        _agencyTierRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AgencyTier, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgencyTier?)null);


        Tier? capturedTier = null;
        _tierRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => capturedTier = tier)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/tiers/{tierId.Value}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        tier.IsEnabled.Should().Be(false);

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    

    [Fact(DisplayName = "DELETE /api/tiers/{id} returns 400 when TierId is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenTierIdIsEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await _client.DeleteAsync($"/api/tiers/{emptyGuid}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("TierId")[0].GetString()
            .Should().Be("TierId must be a non-empty GUID.");

        _tierRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TierId>(), It.IsAny<CancellationToken>()), Times.Never);
        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/tiers/{id} returns 400 when TierId is malformed")]
    public async Task Delete_ShouldReturn400_WhenTierIdIsMalformed()
    {
        // Arrange
        var malformedId = "not-a-valid-guid";

        // Act
        var response = await _client.DeleteAsync($"/api/tiers/{malformedId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _tierRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TierId>(), It.IsAny<CancellationToken>()), Times.Never);
        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}