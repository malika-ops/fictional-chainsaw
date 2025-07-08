using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AgencyTierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.DeleteTests;

public class DeleteAgencyTierEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "DELETE /api/agencyTiers/{id} returns 200 when AgencyTier exists")]
    public async Task Delete_ShouldReturn200_WhenAgencyTierExists()
    {
        // Arrange
        var agencyTierId = new AgencyTierId(Guid.NewGuid());
        var agencyId = new Domain.AgencyAggregate.AgencyId(Guid.NewGuid());
        var tierId = new Domain.TierAggregate.TierId(Guid.NewGuid());

        var agencyTier = AgencyTier.Create(
                            agencyTierId,
                            agencyId,
                            tierId,
                            code: "A12345",
                            password: null);

        _agencyTierRepoMock
            .Setup(r => r.GetByIdAsync(agencyTierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agencyTier);

        AgencyTier? capturedTier = null;
        _agencyTierRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => capturedTier = agencyTier)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/agencyTiers/{agencyTierId.Value}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        capturedTier!.IsEnabled.Should().BeFalse();     
        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/agencyTiers/{id} returns 404 when AgencyTier does not exist")]
    public async Task Delete_ShouldReturn404_WhenAgencyTierNotFound()
    {
        // Arrange
        var nonExistingId = new AgencyTierId(Guid.NewGuid());

        _agencyTierRepoMock
            .Setup(r => r.GetByIdAsync(nonExistingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgencyTier?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/agencyTiers/{nonExistingId.Value}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/agencyTiers/{id} returns 400 when AgencyTierId is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenAgencyTierIdIsEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await _client.DeleteAsync($"/api/agencyTiers/{emptyGuid}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("AgencyTierId")[0].GetString()
            .Should().Be("AgencyTierId must be a non-empty GUID.");

        _agencyTierRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AgencyTierId>(), It.IsAny<CancellationToken>()), Times.Never);
        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/agencyTiers/{id} returns 400 when AgencyTierId is malformed")]
    public async Task Delete_ShouldReturn400_WhenAgencyTierIdIsMalformed()
    {
        // Arrange
        const string malformedId = "not-a-valid-guid";

        // Act
        var response = await _client.DeleteAsync($"/api/agencyTiers/{malformedId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _agencyTierRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AgencyTierId>(), It.IsAny<CancellationToken>()), Times.Never);
        _agencyTierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}