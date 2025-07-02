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
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.GetByIdTests;

public class GetAgencyByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repo = new();

    public GetAgencyByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Agency Make(Guid id, string code = "AGENCY-001", string? name = null, bool enabled = true)
    {
        var agency = Agency.Create(
            id: AgencyId.Of(id),
            code: code,
            name: name ?? $"Agency-{code}",
            abbreviation: code.Substring(0, Math.Min(3, code.Length)),
            address1: "123 Main Street",
            address2: null,
            phone: "+1-555-0123",
            fax: "+1-555-0124",
            accountingSheetName: "Default Sheet",
            accountingAccountNumber: "ACC-001",
            postalCode: "12345",
            latitude: 40.7128m,
            longitude: -74.0060m,
            cashTransporter: null,
            expenseFundAccountingSheet: null,
            expenseFundAccountNumber: null,
            madAccount: null,
            fundingThreshold: null,
            cityId: CityId.Of(Guid.NewGuid()), // Using CityId (required one of City or Sector)
            sectorId: null, // Null because we're using CityId
            agencyTypeId: null,
            tokenUsageStatusId: null,
            fundingTypeId: null,
            partnerId: null,
            supportAccountId: null
        );

        if (!enabled)
            agency.Disable();

        return agency;
    }

    private record AgencyDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/agencies/{id} → 200 when Agency exists")]
    public async Task Get_ShouldReturn200_WhenAgencyExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "AGENCY-123", "Test Agency");

        _repo.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/agencies/{id}");
        var body = await res.Content.ReadFromJsonAsync<AgencyDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(id);
        body.Code.Should().Be("AGENCY-123");
        body.Name.Should().Be("Test Agency");
        body.IsEnabled.Should().BeTrue();

        _repo.Verify(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencies/{id} → 404 when Agency not found")]
    public async Task Get_ShouldReturn404_WhenAgencyNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Agency?)null);

        var res = await _client.GetAsync($"/api/agencies/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencies/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/agencies/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/agencies/{id} → 200 for disabled Agency")]
    public async Task Get_ShouldReturn200_WhenAgencyDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "AGENCY-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/agencies/{id}");
        var dto = await res.Content.ReadFromJsonAsync<AgencyDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}