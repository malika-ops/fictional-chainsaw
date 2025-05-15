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
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.PartnerCountryTests.CreateTests;

public class CreatePartnerCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();

    public CreatePartnerCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var configured = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                /* replace infra with mocks */
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<PartnerCountry>(),
                                           It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerCountry pc, CancellationToken _) => pc);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = configured.CreateClient();
    }

    private static PartnerCountry Make(Guid partnerId, Guid countryId) =>
        PartnerCountry.Create(
            PartnerCountryId.Of(Guid.NewGuid()),
            new PartnerId(partnerId),
            new CountryId(countryId),
            true);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }


    [Fact(DisplayName = "POST /api/partner-countries returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var payload = new
        {
            PartnerId = partnerId,
            CountryId = countryId
        };

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<PartnerCountry>(pc =>
                    pc.PartnerId.Value == partnerId &&
                    pc.CountryId.Value == countryId &&
                    pc.IsEnabled),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "POST /api/partner-countries returns 400 when PartnerId is missing")]
    public async Task Post_ShouldReturn400_WhenPartnerIdMissing()
    {
        var invalid = new       // PartnerId omitted
        {
            CountryId = Guid.NewGuid()
        };

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "PartnerId")
            .Should().Be("PartnerId is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "POST /api/partner-countries returns 400 when CountryId is missing")]
    public async Task Post_ShouldReturn400_WhenCountryIdMissing()
    {
        var invalid = new       // CountryId omitted
        {
            PartnerId = Guid.NewGuid()
        };

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", invalid);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "CountryId")
            .Should().Be("CountryId is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "POST /api/partner-countries returns 400 when pair already exists")]
    public async Task Post_ShouldReturn400_WhenDuplicateExists()
    {
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var existing = Make(partnerId, countryId);

        _repoMock.Setup(r => r.GetByPartnerAndCountryAsync(partnerId,
                                                           countryId,
                                                           It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new
        {
            PartnerId = partnerId,
            CountryId = countryId
        };

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
            .Should().Be($"Partner ({partnerId}) is already linked to Country ({countryId}).");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(),
                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
