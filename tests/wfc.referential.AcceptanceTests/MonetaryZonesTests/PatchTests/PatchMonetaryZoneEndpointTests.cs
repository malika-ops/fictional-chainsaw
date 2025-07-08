using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.PatchTests;

public class PatchMonetaryZoneEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static MonetaryZone Make(Guid id, string code = "OLD", string name = "Old",
                                     string desc = "Desc", bool enabled = true)
    {
        var z = MonetaryZone.Create(MonetaryZoneId.Of(id), code, name, desc);
        if (!enabled) z.Disable();
        return z;
    }

    private static async Task<HttpResponseMessage> PatchJsonAsync(HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage res)
    {
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;
        if (root.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return root.GetBoolean();

        return root.GetProperty("value").GetBoolean();
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} → 200 when patching Code")]
    public async Task Patch_ShouldReturn200_WhenPatchingCode()
    {
        var id = Guid.NewGuid();
        var zone = Make(id, "OLD");

        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(zone);
        _monetaryZoneRepoMock.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync((MonetaryZone?)null);   // unique OK

        var payload = new { MonetaryZoneId = id, Code = "NEW" };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{id}", payload);
        var ok = await ReadBoolAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        zone.Code.Should().Be("NEW");

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} → 200 when patching IsEnabled")]
    public async Task Patch_ShouldReturn200_WhenPatchingIsEnabled()
    {
        var id = Guid.NewGuid();
        var zone = Make(id, enabled: true);

        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(zone);

        var payload = new { MonetaryZoneId = id, IsEnabled = false };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{id}", payload);
        var ok = await ReadBoolAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        zone.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} → 404 when zone missing")]
    public async Task Patch_ShouldReturn400_WhenZoneMissing()
    {
        var id = Guid.NewGuid();

        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((MonetaryZone?)null);

        var payload = new { MonetaryZoneId = id, Code = "X" };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{id}", payload);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} → 409 when duplicate Code")]
    public async Task Patch_ShouldReturn409_WhenDuplicateCode()
    {
        var idTarget = Guid.NewGuid();
        var idOther = Guid.NewGuid();

        var target = Make(idTarget, "OLD");
        var existing = Make(idOther, "DUPL");

        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(idTarget), It.IsAny<CancellationToken>()))
             .ReturnsAsync(target);

        _monetaryZoneRepoMock.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        var payload = new { MonetaryZoneId = idTarget, Code = "DUPL" };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{idTarget}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} → 400 when Code empty string")]
    public async Task Patch_ShouldReturn400_WhenCodeEmpty()
    {
        var id = Guid.NewGuid();
        var payload = new { MonetaryZoneId = id, Code = "" };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} → 400 when Description > 500 chars")]
    public async Task Patch_ShouldReturn400_WhenDescriptionTooLong()
    {
        var id = Guid.NewGuid();
        var big = new string('X', 501);

        var payload = new { MonetaryZoneId = id, Description = big };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/monetaryZones/{id} allows same Code for same zone")]
    public async Task Patch_ShouldAllow_WhenSameCode()
    {
        var id = Guid.NewGuid();
        var zone = Make(id, code: "SAME");

        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(zone);
        _monetaryZoneRepoMock.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<MonetaryZone, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync(zone);     

        var payload = new { MonetaryZoneId = id, Code = "SAME", Name = "New-Name" };

        var res = await PatchJsonAsync(_client, $"/api/monetaryZones/{id}", payload);
        var ok = await ReadBoolAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        zone.Name.Should().Be("New-Name");

        _monetaryZoneRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}