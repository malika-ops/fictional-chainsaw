using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.PatchTests;

public class PatchTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _tierRepo = new();

    public PatchTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<ITierRepository>();
                s.RemoveAll<ICacheService>();

                _tierRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_tierRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Tier MakeTier(Guid id, string name = "Bronze", string description = "Bronze Tier") =>
        Tier.Create(TierId.Of(id), name, description);

    private static async Task<HttpResponseMessage> PatchJsonAsync(
        HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind == JsonValueKind.True || root.ValueKind == JsonValueKind.False)
            return root.GetBoolean();

        if (root.TryGetProperty("value", out var v) &&
            (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False))
            return v.GetBoolean();

        return root.GetBoolean();
    }

    private static string FirstErr(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 200 when patch succeeds")]
    public async Task Patch_ShouldReturn200_WhenPatchSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orig = MakeTier(id, "Bronze", "Bronze tier description");

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(orig);

        _tierRepo.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<Expression<Func<Tier, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        var payload = new
        {
            TierId = id,
            Name = "Silver",
            Description = "Updated silver tier description",
            IsEnabled = false
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify the tier was patched correctly
        orig.Name.Should().Be("Silver");
        orig.Description.Should().Be("Updated silver tier description");
        orig.IsEnabled.Should().BeFalse();

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 200 when patching only name")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orig = MakeTier(id, "Bronze", "Bronze tier description");

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(orig);

        _tierRepo.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<Expression<Func<Tier, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        var payload = new
        {
            TierId = id,
            Name = "Gold"
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only name was changed
        orig.Name.Should().Be("Gold");
        orig.Description.Should().Be("Bronze tier description"); // unchanged
        orig.IsEnabled.Should().BeTrue(); // unchanged

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 200 when patching only description")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyDescription()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orig = MakeTier(id, "Bronze", "Bronze tier description");

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(orig);

        var payload = new
        {
            TierId = id,
            Description = "Updated description only"
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only description was changed
        orig.Name.Should().Be("Bronze"); // unchanged
        orig.Description.Should().Be("Updated description only");
        orig.IsEnabled.Should().BeTrue(); // unchanged

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 200 when patching only IsEnabled")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyIsEnabled()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orig = MakeTier(id, "Bronze", "Bronze tier description");

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(orig);

        var payload = new
        {
            TierId = id,
            IsEnabled = false
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only IsEnabled was changed
        orig.Name.Should().Be("Bronze"); // unchanged
        orig.Description.Should().Be("Bronze tier description"); // unchanged
        orig.IsEnabled.Should().BeFalse();

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 404 when tier not found")]
    public async Task Patch_ShouldReturn404_WhenTierNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        var payload = new { TierId = id, Name = "NonExistent" };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 409 when name already exists")]
    public async Task Patch_ShouldReturn409_WhenNameAlreadyExists()
    {
        // Arrange
        var idTarget = Guid.NewGuid();
        var idExisting = Guid.NewGuid();
        var target = MakeTier(idTarget, "Bronze", "Bronze tier");
        var existing = MakeTier(idExisting, "Silver", "Silver tier");

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(idTarget), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _tierRepo.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<Expression<Func<Tier, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new { TierId = idTarget, Name = "Silver" };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{idTarget}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        doc!.RootElement.GetProperty("errors").GetProperty("message").GetString()
           .Should().Be("Tier name 'Silver' already exists.");

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 400 when TierId is empty GUID")]
    public async Task Patch_ShouldReturn400_WhenTierIdIsEmpty()
    {
        // Arrange
        var body = new { TierId = Guid.Empty, Name = "Invalid" };

        // Act
        var resp = await PatchJsonAsync(
            _client,
            "/api/tiers/00000000-0000-0000-0000-000000000000",
            body);

        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstErr(doc!.RootElement.GetProperty("errors"), "TierId")
            .Should().Be("TierId cannot be empty.");

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 400 when Name is empty string")]
    public async Task Patch_ShouldReturn400_WhenNameIsEmptyString()
    {
        // Arrange
        var id = Guid.NewGuid();
        var body = new { TierId = id, Name = "" };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", body);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstErr(doc!.RootElement.GetProperty("errors"), "Name")
            .Should().Be("Name cannot be empty when provided.");

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 400 when Description is empty string")]
    public async Task Patch_ShouldReturn400_WhenDescriptionIsEmptyString()
    {
        // Arrange
        var id = Guid.NewGuid();
        var body = new { TierId = id, Description = "" };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", body);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstErr(doc!.RootElement.GetProperty("errors"), "Description")
            .Should().Be("Description cannot be empty when provided.");

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    

    [Fact(DisplayName = "PATCH /api/tiers/{id} allows same name for same tier")]
    public async Task Patch_ShouldAllow_WhenSameNameForSameTier()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tier = MakeTier(id, "Bronze", "Bronze tier");

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(tier);

        _tierRepo.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<Expression<Func<Tier, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(tier); // Same tier returned

        var payload = new
        {
            TierId = id,
            Name = "Bronze", // Same name
            Description = "Updated description"
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var result = await ReadBoolAsync(resp);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _tierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}