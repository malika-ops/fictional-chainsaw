using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.DeleteTests;

public class DeleteMonetaryZoneEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static MonetaryZone MakeZone(Guid id, string code = "EU")
    {
        return MonetaryZone.Create(
            MonetaryZoneId.Of(id),
            code,
            name: $"Zone-{code}",
            description: $"Description for {code}"
        );
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 200 when zone exists & has no countries")]
    public async Task Delete_ShouldReturn200_WhenZoneExistsWithoutCountries()
    {
        // Arrange
        var id = Guid.NewGuid();
        var zone = MakeZone(id);                    

        _monetaryZoneRepoMock.Setup(r => r.GetByIdWithIncludesAsync(
                            MonetaryZoneId.Of(id),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(zone);

        MonetaryZone? captured = null;
        _monetaryZoneRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => captured = zone)
                 .Returns(Task.CompletedTask);

        // Act
        var resp = await _client.DeleteAsync($"/api/monetaryZones/{id}");
        var ok = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();
        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 400 when zone has countries")]
    public async Task Delete_ShouldReturn400_WhenZoneHasCountries()
    {
        var id = Guid.NewGuid();
        var zone = MakeZone(id);
        var dummyCountry = _fixture.Create<Country>();
        zone.Countries.Add(dummyCountry);

        _monetaryZoneRepoMock.Setup(r => r.GetByIdWithIncludesAsync(
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

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} → 404 when zone not found")]
    public async Task Delete_ShouldReturn400_WhenZoneMissing()
    {
        var id = Guid.NewGuid();

        _monetaryZoneRepoMock.Setup(r => r.GetByIdWithIncludesAsync(
                            MonetaryZoneId.Of(id),
                            It.IsAny<CancellationToken>(),
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync((MonetaryZone?)null);

        var resp = await _client.DeleteAsync($"/api/monetaryZones/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        _monetaryZoneRepoMock.Verify(r => r.GetByIdWithIncludesAsync(
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

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}