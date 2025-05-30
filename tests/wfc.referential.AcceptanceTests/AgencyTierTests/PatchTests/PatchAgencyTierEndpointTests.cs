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
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.PatchTests;

public class PatchAgencyTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _agencyTierRepo = new();
    private readonly Mock<IAgencyRepository> _agencyRepo = new();
    private readonly Mock<ITierRepository> _tierRepo = new();

    public PatchAgencyTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyTierRepository>();
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ITierRepository>();
                s.RemoveAll<ICacheService>();

                _agencyTierRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

                s.AddSingleton(_agencyTierRepo.Object);
                s.AddSingleton(_agencyRepo.Object);
                s.AddSingleton(_tierRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static AgencyTier MakeAgencyTier(Guid id, Guid agencyId,
                                             Guid tierId, string code = "CODE1",
                                             string pwd = "pwd", bool en = true) =>
        AgencyTier.Create(AgencyTierId.Of(id),
                          AgencyId.Of(agencyId),
                          TierId.Of(tierId),
                          code,
                          pwd);

    private static async Task<HttpResponseMessage> PatchJsonAsync(
        HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;

        if (root.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return root.GetBoolean();

        if (root.TryGetProperty("value", out var v) &&
            (v.ValueKind is JsonValueKind.True or JsonValueKind.False))
            return v.GetBoolean();

        return root.GetBoolean();
    }

    
    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 200 when patching only Code")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyCode()
    {
        var id = Guid.NewGuid();
        var orig = MakeAgencyTier(id, Guid.NewGuid(), Guid.NewGuid(), "OLD");

        _agencyTierRepo.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(orig);

        _agencyTierRepo.Setup(r => r.GetOneByConditionAsync(
                                 It.IsAny<Expression<Func<AgencyTier, bool>>>(),
                                 It.IsAny<CancellationToken>()))
                       .ReturnsAsync((AgencyTier?)null);

        var payload = new { AgencyTierId = id, Code = "NEW" };

        var resp = await PatchJsonAsync(_client, $"/api/agencyTiers/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        orig.Code.Should().Be("NEW");
        orig.IsEnabled.Should().BeTrue();      
        _agencyTierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 200 when patching only IsEnabled")]
    public async Task Patch_ShouldReturn200_WhenPatchingOnlyIsEnabled()
    {
        var id = Guid.NewGuid();
        var orig = MakeAgencyTier(id, Guid.NewGuid(), Guid.NewGuid());

        _agencyTierRepo.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(orig);

        var payload = new { AgencyTierId = id, IsEnabled = false };

        var resp = await PatchJsonAsync(_client, $"/api/agencyTiers/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        orig.IsEnabled.Should().BeFalse();
        orig.Code.Should().Be("CODE1");   
        _agencyTierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 404 when AgencyTier not found")]
    public async Task Patch_ShouldReturn404_WhenAgencyTierNotFound()
    {
        var id = Guid.NewGuid();

        _agencyTierRepo.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((AgencyTier?)null);

        var payload = new { AgencyTierId = id, Code = "NOPE" };

        var resp = await PatchJsonAsync(_client, $"/api/agencyTiers/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _agencyTierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 404 when new TierId not found")]
    public async Task Patch_ShouldReturn404_WhenNewTierNotFound()
    {
        var id = Guid.NewGuid();
        var newTid = Guid.NewGuid();
        var orig = MakeAgencyTier(id, Guid.NewGuid(), Guid.NewGuid());

        _agencyTierRepo.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(orig);

        _tierRepo.Setup(r => r.GetByIdAsync(TierId.Of(newTid), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Domain.TierAggregate.Tier?)null); // missing

        var payload = new { AgencyTierId = id, TierId = newTid };

        var resp = await PatchJsonAsync(_client, $"/api/agencyTiers/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _agencyTierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 404 when new AgencyId not found")]
    public async Task Patch_ShouldReturn404_WhenNewAgencyNotFound()
    {
        var id = Guid.NewGuid();
        var newAid = Guid.NewGuid();
        var orig = MakeAgencyTier(id, Guid.NewGuid(), Guid.NewGuid());

        _agencyTierRepo.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(orig);

        _agencyRepo.Setup(r => r.GetByIdAsync(AgencyId.Of(newAid), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Domain.AgencyAggregate.Agency?)null); // missing

        var payload = new { AgencyTierId = id, AgencyId = newAid };

        var resp = await PatchJsonAsync(_client, $"/api/agencyTiers/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _agencyTierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} allows same Code for same entity")]
    public async Task Patch_ShouldAllow_WhenSameCodeSameEntity()
    {
        var id = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var entity = MakeAgencyTier(id, agencyId, tierId, "CODEX");

        _agencyTierRepo.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(entity);

        _agencyTierRepo.Setup(r => r.GetOneByConditionAsync(
                                 It.IsAny<Expression<Func<AgencyTier, bool>>>(),
                                 It.IsAny<CancellationToken>()))
                       .ReturnsAsync(entity); // same entity returned → allowed

        var payload = new { AgencyTierId = id, Code = "CODEX" };

        var resp = await PatchJsonAsync(_client, $"/api/agencyTiers/{id}", payload);
        var result = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _agencyTierRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}