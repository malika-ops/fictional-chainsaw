using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.CreateTests;

public class CreateTierEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "POST /api/tiers returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Name = "Premium Tier",
            Description = "High-level tier for premium services"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _tierRepoMock.Verify(r =>
            r.AddAsync(It.Is<Tier>(t =>
                    t.Name == payload.Name &&
                    t.Description == payload.Description),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/tiers returns 400 & problem‑details when Name is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var invalidPayload = new
        {
            // Name intentionally omitted to trigger validation error
            Description = "Some description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name is required.");

        // the handler must NOT be reached
        _tierRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/tiers returns 400 & problem‑details when Name is empty")]
    public async Task Post_ShouldReturn400_WhenNameIsEmpty()
    {
        // Arrange
        var invalidPayload = new
        {
            Name = "",
            Description = "Some description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name is required.");

        _tierRepoMock.Verify(r => r.AddAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "POST /api/tiers returns 400 & problem‑details when Name exceeds max length")]
    public async Task Post_ShouldReturn400_WhenNameExceedsMaxLength()
    {
        // Arrange: Name with 101 characters (exceeds 100 char limit)
        var longName = new string('A', 101);
        var invalidPayload = new
        {
            Name = longName,
            Description = "Some description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name max length is 100 chars.");

        _tierRepoMock.Verify(r => r.AddAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "POST /api/tiers returns 409 when Name already exists")]
    public async Task Post_ShouldReturn409_WhenNameAlreadyExists()
    {
        // Arrange 
        const string duplicateName = "Premium Tier";

        // Tell the repo mock that the name already exists
        var existingTier = Tier.Create(
            new TierId(Guid.NewGuid()),
            duplicateName,
            "Existing premium tier");

        _tierRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tier, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTier);

        var payload = new
        {
            Name = duplicateName,
            Description = "New premium tier"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetProperty("message").GetString();

        error.Should().Be($"Tier with name '{duplicateName}' already exists.");

        _tierRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the name is already taken");

        _tierRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                         Times.Never,
                         "no save should happen when the name is already taken");
    }

    [Fact(DisplayName = "POST /api/tiers returns 409 when Name already exists (case insensitive)")]
    public async Task Post_ShouldReturn409_WhenNameAlreadyExists_CaseInsensitive()
    {
        // Arrange 
        const string existingName = "Premium Tier";
        const string duplicateNameDifferentCase = "PREMIUM TIER";

        // Tell the repo mock that the name already exists (case insensitive match)
        var existingTier = Tier.Create(
            new TierId(Guid.NewGuid()),
            existingName,
            "Existing premium tier");

        _tierRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tier, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTier);

        var payload = new
        {
            Name = duplicateNameDifferentCase,
            Description = "New premium tier"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetProperty("message").GetString();

        error.Should().Be($"Tier with name '{duplicateNameDifferentCase}' already exists.");

        // Handler must NOT attempt to add the entity
        _tierRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the name is already taken (case insensitive)");
    }

    [Fact(DisplayName = "POST /api/tiers accepts request with only Name (Description optional)")]
    public async Task Post_ShouldReturn200_WhenDescriptionIsEmpty()
    {
        // Arrange
        var payload = new
        {
            Name = "Basic Tier"
            // Description intentionally omitted
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tiers", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        _tierRepoMock.Verify(r =>
            r.AddAsync(It.Is<Tier>(t =>
                    t.Name == payload.Name &&
                    t.Description == string.Empty), // Description should default to empty string
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}