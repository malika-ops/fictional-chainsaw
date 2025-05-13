using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyTierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.CreateTests;

public class CreateAgencyTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _repoMock = new();

    public CreateAgencyTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyTierRepository>();
                s.RemoveAll<ICacheService>();

                // default → no duplicate
                _repoMock.Setup(r => r.ExistsAsync(
                                        It.IsAny<Guid>(),
                                        It.IsAny<Guid>(),
                                        It.IsAny<string>(),
                                        It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

                _repoMock.Setup(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                                It.IsAny<CancellationToken>()))
                         .ReturnsAsync((AgencyTier at, CancellationToken _) => at);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------- tests ---------- */

    // 1) Happy-path ----------------------------------------------------------
    [Fact(DisplayName = "POST /api/agencyTiers returns 200 and Guid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var payload = new
        {
            AgencyId = agencyId,
            TierId = tierId,
            Code = "AT-001",
            Password = "secret"
        };

        // Act
        var resp = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<AgencyTier>(at =>
                    at.AgencyId.Value == agencyId &&
                    at.TierId.Value == tierId &&
                    at.Code == payload.Code &&
                    at.Password == payload.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // 2) Validation – Code missing ------------------------------------------
    [Fact(DisplayName = "POST /api/agencyTiers returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeMissing()
    {
        var payload = new
        {
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid()
            // Code omitted
        };

        var resp = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "code")
            .Should().Be("Code is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 3) Duplicate (Agency, Tier, Code) --------------------------------------
    [Fact(DisplayName = "POST /api/agencyTiers returns 400 when Code already exists for this Agency & Tier")]
    public async Task Post_ShouldReturn400_WhenCodeDuplicate()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        const string dup = "DUP-01";

        _repoMock.Setup(r => r.ExistsAsync(agencyId, tierId, dup, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var payload = new
        {
            AgencyId = agencyId,
            TierId = tierId,
            Code = dup,
            Password = "pwd"
        };

        // Act
        var resp = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"AgencyTier with code '{dup}' already exists for this agency & tier.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 4) Empty GUIDs ---------------------------------------------------------
    [Fact(DisplayName = "POST /api/agencyTiers returns 400 when AgencyId is empty GUID")]
    public async Task Post_ShouldReturn400_WhenAgencyIdEmpty()
    {
        var payload = new
        {
            AgencyId = Guid.Empty,
            TierId = Guid.NewGuid(),
            Code = "X"
        };

        var resp = await _client.PostAsJsonAsync("/api/agencyTiers", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "AgencyId")
            .Should().Be("AgencyId cannot be empty.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<AgencyTier>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}