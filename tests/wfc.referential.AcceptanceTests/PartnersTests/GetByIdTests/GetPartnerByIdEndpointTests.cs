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
using wfc.referential.Domain.PartnerAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.GetByIdTests;

public class GetPartnerByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repo = new();

    public GetPartnerByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Partner Make(Guid id, string code = "PARTNER-001", string? name = null, bool enabled = true)
    {
        var partner = Partner.Create(
            id: PartnerId.Of(id),
            code: code,
            name: name ?? $"Partner-{code}",
            personType: "Company",
            professionalTaxNumber: "PTN-001",
            withholdingTaxRate: "5%",
            headquartersCity: "New York",
            headquartersAddress: "123 Main St",
            lastName: "Smith",
            firstName: "John",
            phoneNumberContact: "+1-555-0123",
            mailContact: "contact@partner.com",
            functionContact: "Manager",
            transferType: "Electronic",
            authenticationMode: "Standard",
            taxIdentificationNumber: "TIN-001",
            taxRegime: "Standard",
            auxiliaryAccount: "AUX-001",
            ice: "ICE-001",
            logo: "logo.png"
        );

        if (!enabled)
            partner.Disable();

        return partner;
    }

    private record PartnerDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/partners/{id} → 404 when Partner not found")]
    public async Task Get_ShouldReturn404_WhenPartnerNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Partner?)null);

        var res = await _client.GetAsync($"/api/partners/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/partners/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/partners/{id} → 200 for disabled Partner")]
    public async Task Get_ShouldReturn200_WhenPartnerDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PARTNER-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/partners/{id}");
        var dto = await res.Content.ReadFromJsonAsync<PartnerDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}