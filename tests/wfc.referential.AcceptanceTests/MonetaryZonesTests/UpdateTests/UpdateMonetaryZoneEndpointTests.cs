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
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.UpdateTests;

public class UpdateMonetaryZoneEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repoMock = new();

    public UpdateMonetaryZoneEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
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

        _client = custom.CreateClient();
    }

    private static MonetaryZone Make(Guid id, string code = "OLD", string name = "Old-Name",
                                     string desc = "Old desc", bool enabled = true)
    {
        var zone = MonetaryZone.Create(MonetaryZoneId.Of(id), code, name, desc);
        if (!enabled) zone.Disable();
        return zone;
    }

    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        var id = Guid.NewGuid();
        var zone = Make(id);

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(zone);

        _repoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((MonetaryZone?)null);  

        var payload = new
        {
            MonetaryZoneId = id,
            Code = "NEW",
            Name = "New-Name",
            Description = "Updated desc",
            IsEnabled = false
        };

        var res = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        zone.Code.Should().Be("NEW");
        zone.Name.Should().Be("New-Name");
        zone.Description.Should().Be("Updated desc");
        zone.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 400 when Code > 50 chars")]
    public async Task Put_ShouldReturn400_WhenCodeTooLong()
    {
        var id = Guid.NewGuid();
        var longCode = new string('X', 51);

        var payload = new
        {
            MonetaryZoneId = id,
            Code = longCode,
            Name = "N",
            Description = "D"
        };

        var res = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 400 when zone missing")]
    public async Task Put_ShouldReturn400_WhenZoneNotFound()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((MonetaryZone?)null);

        var payload = new { MonetaryZoneId = id, Code = "X", Name = "X" };

        var res = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 409 when duplicate Code exists")]
    public async Task Put_ShouldReturn409_WhenDuplicateCode()
    {
        var idTarget = Guid.NewGuid();
        var idOther = Guid.NewGuid();

        var target = Make(idTarget, code: "OLD");
        var existing = Make(idOther, code: "DUPL");

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(idTarget), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new { MonetaryZoneId = idTarget, Code = "DUPL", Name = "X" };

        var res = await _client.PutAsJsonAsync($"/api/monetaryZones/{idTarget}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 200 when disabling zone")]
    public async Task Put_ShouldReturn200_WhenDisabling()
    {
        var id = Guid.NewGuid();
        var zone = Make(id, enabled: true);

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(zone);

        _repoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((MonetaryZone?)null);

        var payload = new { MonetaryZoneId = id, Code = "OLD", Name = "Old-Name", IsEnabled = false };

        var res = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        zone.IsEnabled.Should().BeFalse();
    }


    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 200 when keeping same Code")]
    public async Task Put_ShouldReturn200_WhenKeepingSameCode()
    {
        var id = Guid.NewGuid();
        var zone = Make(id, code: "SAME");

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(zone);

        _repoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(zone);   

        var payload = new { MonetaryZoneId = id, Code = "SAME", Name = "New-Name" };

        var res = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        zone.Name.Should().Be("New-Name");
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "PUT /api/monetaryZones/{id} → 400 when MonetaryZoneId empty")]
    public async Task Put_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new { MonetaryZoneId = Guid.Empty, Code = "X", Name = "Y" };

        var res = await _client.PutAsJsonAsync(
            "/api/monetaryZones/00000000-0000-0000-0000-000000000000",
            payload);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}