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
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.UpdateTests;

public class UpdateAgencyTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _repoMock = new();

    public UpdateAgencyTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyTierRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<AgencyTier>(),
                                                   It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static AgencyTier At(Guid id, Guid agencyId, Guid tierId, string code, string pwd = "pwd", bool enabled = true) =>
        AgencyTier.Create(
            AgencyTierId.Of(id),
            new AgencyId(agencyId),
            new TierId(tierId),
            code,
            pwd,
            enabled);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------- tests ---------- */

    // ─────────── Happy-path ───────────
    [Fact(DisplayName = "PUT /api/agencyTiers/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var existing = At(id, agencyId, tierId, "OLD", "pwd", true);

        _repoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _repoMock.Setup(r => r.GetByCodeAsync("NEWCODE", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((AgencyTier?)null);

        AgencyTier? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()))
                 .Callback<AgencyTier, CancellationToken>((at, _) => saved = at)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            AgencyTierId = id,
            AgencyId = agencyId,
            TierId = tierId,
            Code = "NEWCODE",
            Password = "newpwd",
            IsEnabled = false
        };

        // Act
        var resp = await _client.PutAsJsonAsync($"/api/agencyTiers/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.Code.Should().Be("NEWCODE");
        saved.Password.Should().Be("newpwd");
        saved.IsEnabled.Should().BeFalse();
        saved.AgencyId.Value.Should().Be(agencyId);
        saved.TierId.Value.Should().Be(tierId);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // ─────────── Validation – Code missing ───────────
    [Fact(DisplayName = "PUT /api/agencyTiers/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        var id = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var invalid = new   // Code property omitted
        {
            AgencyTierId = id,
            AgencyId = agencyId,
            TierId = tierId,
            Password = "pwd"
        };

        var resp = await _client.PutAsJsonAsync($"/api/agencyTiers/{id}", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "code")
            .Should().Be("Code is required.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // ─────────── Business rule – duplicate Code ───────────
    [Fact(DisplayName = "PUT /api/agencyTiers/{id} returns 400 when Code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeDuplicate()
    {
        var id = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var target = At(id, agencyId, tierId, "TARGET");
        var duplicate = At(Guid.NewGuid(), agencyId, tierId, "DUP");

        _repoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("DUP", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicate);

        var payload = new
        {
            AgencyTierId = id,
            AgencyId = agencyId,
            TierId = tierId,
            Code = "DUP",
            Password = "pwd"
        };

        var resp = await _client.PutAsJsonAsync($"/api/agencyTiers/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("AgencyTier with code 'DUP' already exists for this agency & tier.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // ─────────── Not-found ───────────
    [Fact(DisplayName = "PUT /api/agencyTiers/{id} returns 404 when AgencyTier is not found")]
    public async Task Put_ShouldReturn404_WhenEntityMissing()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((AgencyTier?)null);

        var payload = new
        {
            AgencyTierId = id,
            AgencyId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            Code = "CODE",
            Password = "pwd"
        };

        var resp = await _client.PutAsJsonAsync($"/api/agencyTiers/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}