using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.UpdateTests;

public class UpdateTierEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    // helper to create a Tier quickly
    private static Tier CreateTier(Guid id, string name, string description, bool isEnabled = true)
    {
        var tier = Tier.Create(TierId.Of(id), name, description);
        if (!isEnabled)
        {
            tier.Disable();
        }
        return tier;
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTier = CreateTier(id, "Bronze Tier", "Basic tier description");

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTier);

        _tierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tier, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);   

        var payload = new
        {
            TierId = id,
            Name = "Gold Tier",
            Description = "Premium tier with advanced features",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        existingTier.Name.Should().Be("Gold Tier");
        existingTier.Description.Should().Be("Premium tier with advanced features");
        existingTier.IsEnabled.Should().BeTrue();

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 400 when Name exceeds max length")]
    public async Task Put_ShouldReturn400_WhenNameExceedsMaxLength()
    {
        // Arrange
        var id = Guid.NewGuid();
        var longName = new string('A', 101); // 101 characters, exceeds 100 limit

        var payload = new
        {
            TierId = id,
            Name = longName,
            Description = "Some description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name max length is 100 chars.");

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 400 when TierId is empty")]
    public async Task Put_ShouldReturn400_WhenTierIdIsEmpty()
    {
        // Arrange
        var payload = new
        {
            TierId = Guid.Empty,
            Name = "Valid Name",
            Description = "Some description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{Guid.Empty}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 404 when Tier not found")]
    public async Task Put_ShouldReturn404_WhenTierNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        var payload = new
        {
            TierId = id,
            Name = "Valid Name",
            Description = "Some description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 409 when new name already exists")]
    public async Task Put_ShouldReturn409_WhenNameAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var existingTier = CreateTier(id, "Silver Tier", "Original tier");
        var conflictingTier = CreateTier(otherId, "Gold Tier", "Existing gold tier");

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTier);

        _tierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tier, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(conflictingTier); // name already exists

        var payload = new
        {
            TierId = id,
            Name = "Gold Tier", // conflicts with existing tier
            Description = "Updated description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 200 when disabling tier")]
    public async Task Put_ShouldReturn200_WhenDisablingTier()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTier = CreateTier(id, "Active Tier", "Currently active tier", true);

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTier);

        _tierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tier, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        var payload = new
        {
            TierId = id,
            Name = "Disabled Tier",
            Description = "This tier is now disabled",
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        existingTier.Name.Should().Be("Disabled Tier");
        existingTier.Description.Should().Be("This tier is now disabled");
        existingTier.IsEnabled.Should().BeFalse();

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/tiers/{id} returns 200 when updating with same name")]
    public async Task Put_ShouldReturn200_WhenUpdatingWithSameName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTier = CreateTier(id, "Platinum Tier", "Original description");

        _tierRepoMock.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTier);

        // Return the same tier when checking for name conflicts
        _tierRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tier, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTier);

        var payload = new
        {
            TierId = id,
            Name = "Platinum Tier", // same name
            Description = "Updated description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        existingTier.Name.Should().Be("Platinum Tier");
        existingTier.Description.Should().Be("Updated description");

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}