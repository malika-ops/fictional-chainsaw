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

namespace wfc.referential.AcceptanceTests.PartnerCountryTests.UpdateTests;

public class UpdatePartnerCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();

    public UpdatePartnerCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                                   It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    private static PartnerCountry Pc(Guid id, Guid partnerId, Guid countryId, bool enabled = true) =>
        PartnerCountry.Create(
            PartnerCountryId.Of(id),
            new PartnerId(partnerId),
            new CountryId(countryId),
            enabled);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }


    [Fact(DisplayName = "PUT /api/partnerCountries/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        var id = Guid.NewGuid();
        var oldPartnerId = Guid.NewGuid();
        var oldCountryId = Guid.NewGuid();
        var entity = Pc(id, oldPartnerId, oldCountryId);

        var newPartnerId = Guid.NewGuid();
        var newCountryId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

        _repoMock.Setup(r => r.GetByPartnerAndCountryAsync(newPartnerId, newCountryId,
                                                           It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerCountry?)null);

        PartnerCountry? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                           It.IsAny<CancellationToken>()))
                 .Callback<PartnerCountry, CancellationToken>((pc, _) => saved = pc);

        var payload = new
        {
            PartnerCountryId = id,
            PartnerId = newPartnerId,
            CountryId = newCountryId,
            IsEnabled = false
        };

        var resp = await _client.PutAsJsonAsync($"/api/partnerCountries/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.PartnerId.Value.Should().Be(newPartnerId);
        saved.CountryId.Value.Should().Be(newCountryId);
        saved.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partnerCountries/{id} returns 400 when partner/country pair already exists")]
    public async Task Put_ShouldReturn400_WhenDuplicatePair()
    {
        var idTarget = Guid.NewGuid();
        var partnerIdNew = Guid.NewGuid();
        var countryIdNew = Guid.NewGuid();

        var target = Pc(idTarget, Guid.NewGuid(), Guid.NewGuid());
        var existing = Pc(Guid.NewGuid(), partnerIdNew, countryIdNew);

        _repoMock.Setup(r => r.GetByIdAsync(idTarget, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByPartnerAndCountryAsync(partnerIdNew, countryIdNew,
                                                           It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new
        {
            PartnerCountryId = idTarget,
            PartnerId = partnerIdNew,
            CountryId = countryIdNew,
            IsEnabled = true
        };

        var resp = await _client.PutAsJsonAsync($"/api/partnerCountries/{idTarget}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Partner ({partnerIdNew}) is already linked to Country ({countryIdNew}).");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partnerCountries/{id} returns 400 when PartnerCountryId is empty GUID")]
    public async Task Put_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new
        {
            PartnerCountryId = Guid.Empty,
            PartnerId = Guid.NewGuid(),
            CountryId = Guid.NewGuid()
        };

        var resp = await _client.PutAsJsonAsync(
                       "/api/partnerCountries/00000000-0000-0000-0000-000000000000",
                       payload);

        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // no custom message → just assert error key exists
        FirstError(doc!.RootElement.GetProperty("errors"), "PartnerCountryId")
            .Should().NotBeNullOrWhiteSpace();

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partnerCountries/{id} returns 400 when PartnerCountry not found")]
    public async Task Put_ShouldReturn400_WhenEntityMissing()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerCountry?)null);

        var payload = new
        {
            PartnerCountryId = id,
            PartnerId = Guid.NewGuid(),
            CountryId = Guid.NewGuid()
        };

        var resp = await _client.PutAsJsonAsync($"/api/partnerCountries/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("PartnerCountry not found");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}