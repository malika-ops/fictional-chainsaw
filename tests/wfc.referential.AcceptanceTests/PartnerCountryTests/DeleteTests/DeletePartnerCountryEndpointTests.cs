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


namespace wfc.referential.AcceptanceTests.PartnerCountryTests.DeleteTests;

public class DeletePartnerCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();

    public DeletePartnerCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    private static PartnerCountry MakeLink(Guid id, Guid partnerId, Guid countryId, bool enabled = true)
    {
        var pc = PartnerCountry.Create(
                    PartnerCountryId.Of(id),
                    PartnerId.Of(partnerId),
                    CountryId.Of(countryId));

        if (!enabled) pc.Disable();
        return pc;
    }


    [Fact(DisplayName = "DELETE /api/partner-countries/{id} → 200 when link exists")]
    public async Task Delete_ShouldReturn200_WhenLinkExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var link = MakeLink(id, partnerId, countryId);

        _repoMock.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(link);

        PartnerCountry? captured = null;
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => captured = link)
                 .Returns(Task.CompletedTask);

        // Act
        var resp = await _client.DeleteAsync($"/api/partner-countries/{id}");
        var ok = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();               
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partner-countries/{id} → 404 when link not found")]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerCountry?)null);

        var resp = await _client.DeleteAsync($"/api/partner-countries/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partner-countries/{id} → 400 when id is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenIdEmpty()
    {
        var empty = Guid.Empty;

        var resp = await _client.DeleteAsync($"/api/partner-countries/{empty}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("errors")
            .GetProperty("PartnerCountryId")[0].GetString()
            .Should().Be("PartnerCountryId must be a non-empty GUID.");

        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<PartnerCountryId>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partner-countries/{id} → 400 when id is malformed GUID")]
    public async Task Delete_ShouldReturn400_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var resp = await _client.DeleteAsync($"/api/partner-countries/{bad}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}