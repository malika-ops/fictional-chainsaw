using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.DeleteTests;

public class DeleteMonetaryZoneEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repoMock = new();

    public DeleteMonetaryZoneEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IMonetaryZoneRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    private static MonetaryZone MakeZone(Guid id, string code = "EU")
    {
        return MonetaryZone.Create(
            MonetaryZoneId.Of(id),
            code,
            name: $"Zone-{code}",
            description: $"Description for {code}"
        );
    }

    private static Country DummyCountry()
        => FormatterServices.GetUninitializedObject(typeof(Country)) as Country
           ?? throw new InvalidOperationException("Failed to create dummy Country");


    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 200 when zone exists & has no countries")]
    public async Task Delete_ShouldReturn200_WhenZoneExistsWithoutCountries()
    {
        // Arrange
        var id = Guid.NewGuid();
        var zone = MakeZone(id);                    

        _repoMock.Setup(r => r.GetByIdWithIncludesAsync(
                            MonetaryZoneId.Of(id),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(zone);

        MonetaryZone? captured = null;
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => captured = zone)
                 .Returns(Task.CompletedTask);

        // Act
        var resp = await _client.DeleteAsync($"/api/monetaryZones/{id}");
        var ok = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 400 when zone has countries")]
    public async Task Delete_ShouldReturn400_WhenZoneHasCountries()
    {
        var id = Guid.NewGuid();
        var zone = MakeZone(id);
        zone.Countries.Add(DummyCountry());

        _repoMock.Setup(r => r.GetByIdWithIncludesAsync(
                            MonetaryZoneId.Of(id),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(zone);

        // Act
        var resp = await _client.DeleteAsync($"/api/monetaryZones/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 404 when zone not found")]
    public async Task Delete_ShouldReturn400_WhenZoneMissing()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdWithIncludesAsync(
                            MonetaryZoneId.Of(id),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync((MonetaryZone?)null);

        var resp = await _client.DeleteAsync($"/api/monetaryZones/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 400 when id is Guid.Empty")]
    public async Task Delete_ShouldReturn400_WhenIdEmpty()
    {
        var empty = Guid.Empty;

        var resp = await _client.DeleteAsync($"/api/monetaryZones/{empty}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("MonetaryZoneId")[0].GetString()
           .Should().Be("MonetaryZoneId must be a non-empty GUID.");

        _repoMock.Verify(r => r.GetByIdWithIncludesAsync(
                            It.IsAny<MonetaryZoneId>(),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, object>>[]>()),
                         Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 400 when id is malformed")]
    public async Task Delete_ShouldReturn400_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var resp = await _client.DeleteAsync($"/api/monetaryZones/{bad}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}