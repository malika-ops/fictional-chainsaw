using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceControleTests.UpdateTests;

public class UpdateServiceControleEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static ServiceControle Make(Guid id, Guid svc, Guid ctl,
                                        Guid chn, int exec = 0, bool enabled = true)
    {
        var sc = ServiceControle.Create(
            ServiceControleId.Of(id),
            ServiceId.Of(svc),
            ControleId.Of(ctl),
            ParamTypeId.Of(chn),
            exec);

        if (!enabled) sc.Disable();
        return sc;
    }

    private static async Task<bool> ReadBool(HttpResponseMessage r) =>
        await r.Content.ReadFromJsonAsync<bool>();


    [Fact(DisplayName = "PUT /api/serviceControles/{id} → 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenSuccess()
    {
        var scId = Guid.NewGuid();
        var svcId = Guid.NewGuid();
        var ctlId = Guid.NewGuid();
        var chnId = Guid.NewGuid();

        var existing = Make(scId, svcId, ctlId, chnId, exec: 0, enabled: true);

        _serviceControlRepoMock.Setup(r => r.GetByIdAsync(ServiceControleId.Of(scId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(svcId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Service>());

        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(ctlId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Controle>());

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chnId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<ParamType>());

        _serviceControlRepoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceControle, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ServiceControle?)null);   // unique

        var payload = new
        {
            ServiceControleId = scId,
            ServiceId = svcId,
            ControleId = ctlId,
            ChannelId = chnId,
            ExecOrder = 5,
            IsEnabled = false
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{scId}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        existing.ExecOrder.Should().Be(5);
        existing.IsEnabled.Should().BeFalse();
        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT keeps same (Service,Controle,Channel) tuple → 200")]
    public async Task Put_ShouldReturn200_WhenKeepingSameTuple()
    {
        var scId = Guid.NewGuid();
        var svcId = Guid.NewGuid();
        var ctlId = Guid.NewGuid();
        var chnId = Guid.NewGuid();

        var entity = Make(scId, svcId, ctlId, chnId);

        _serviceControlRepoMock.Setup(r => r.GetByIdAsync(ServiceControleId.Of(scId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(svcId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Service>());
        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(ctlId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Controle>());
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chnId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<ParamType>());

        // uniqueness check returns the *same* row
        _serviceControlRepoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceControle, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

        var payload = new
        {
            ServiceControleId = scId,
            ServiceId = svcId,
            ControleId = ctlId,
            ChannelId = chnId,
            ExecOrder = 9,
            IsEnabled = true
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{scId}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        entity.ExecOrder.Should().Be(9);
    }


    [Theory(DisplayName = "PUT → 400 when any ID is empty")]
    [InlineData("ServiceId")]
    [InlineData("ControleId")]
    [InlineData("ChannelId")]
    [InlineData("ServiceControleId")]
    public async Task Put_ShouldReturn400_WhenIdEmpty(string which)
    {
        var scId = Guid.NewGuid();
        var svcId = Guid.NewGuid();
        var ctlId = Guid.NewGuid();
        var chnId = Guid.NewGuid();

        Guid empty = Guid.Empty;

        var payload = new
        {
            ServiceControleId = which == "ServiceControleId" ? empty : scId,
            ServiceId = which == "ServiceId" ? empty : svcId,
            ControleId = which == "ControleId" ? empty : ctlId,
            ChannelId = which == "ChannelId" ? empty : chnId,
            ExecOrder = 0
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{payload.ServiceControleId}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT → 400 when ExecOrder < 0")]
    public async Task Put_ShouldReturn400_WhenExecOrderNegative()
    {
        var id = Guid.NewGuid();

        var payload = new
        {
            ServiceControleId = id,
            ServiceId = Guid.NewGuid(),
            ControleId = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            ExecOrder = -5
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
           .GetProperty("ExecOrder")[0].GetString()
           .Should().Be("ExecOrder must be ≥ 0.");

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT → 404 when ServiceControle not found")]
    public async Task Put_ShouldReturn404_WhenLinkMissing()
    {
        var id = Guid.NewGuid();

        _serviceControlRepoMock.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ServiceControle?)null);

        var payload = new { ServiceControleId = id, ServiceId = Guid.NewGuid(), ControleId = Guid.NewGuid(), ChannelId = Guid.NewGuid() };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "PUT → 404 when Service / Controle / Channel not found")]
    [InlineData("Service")]
    [InlineData("Controle")]
    [InlineData("Channel")]
    public async Task Put_ShouldReturn404_WhenReferenceMissing(string missing)
    {
        var scId = Guid.NewGuid();
        var svcId = Guid.NewGuid();
        var ctlId = Guid.NewGuid();
        var chnId = Guid.NewGuid();

        var link = Make(scId, svcId, ctlId, chnId);
        _serviceControlRepoMock.Setup(r => r.GetByIdAsync(ServiceControleId.Of(scId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(link);

        if (missing != "Service")
            _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(svcId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_fixture.Create<Service>());

        if (missing != "Controle")
            _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(ctlId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_fixture.Create<Controle>());

        if (missing != "Channel")
            _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chnId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_fixture.Create<ParamType>());

        var payload = new
        {
            ServiceControleId = scId,
            ServiceId = svcId,
            ControleId = ctlId,
            ChannelId = chnId,
            ExecOrder = 1
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{scId}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT → 409 when another row has same Service+Controle+Channel")]
    public async Task Put_ShouldReturn409_WhenDuplicateTuple()
    {
        var scId1 = Guid.NewGuid();
        var scId2 = Guid.NewGuid();
        var svcId = Guid.NewGuid();
        var ctlId = Guid.NewGuid();
        var chnId = Guid.NewGuid();

        var target = Make(scId1, svcId, ctlId, chnId);
        var duplicate = Make(scId2, svcId, ctlId, chnId);

        _serviceControlRepoMock.Setup(r => r.GetByIdAsync(ServiceControleId.Of(scId1), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(svcId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Service>());
        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(ctlId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Controle>());
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chnId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<ParamType>());

        _serviceControlRepoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceControle, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicate);  // ← other row conflicting

        var payload = new
        {
            ServiceControleId = scId1,
            ServiceId = svcId,
            ControleId = ctlId,
            ChannelId = chnId,
            ExecOrder = 2
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{scId1}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT → 404 when route id is malformed")]
    public async Task Put_ShouldReturn404_WhenRouteMalformed()
    {
        const string badRouteId = "not-a-guid";

        var payload = new
        {
            ServiceControleId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            ControleId = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            ExecOrder = 0
        };

        var res = await _client.PutAsJsonAsync($"/api/serviceControles/{badRouteId}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}