using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceControleTests.CreateTests;

public class CreateServiceControleEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static object ValidPayload(Guid? svc = null, Guid? ctrl = null,
                                       Guid? chan = null, int exec = 0) =>
        new
        {
            ServiceId = svc ?? Guid.NewGuid(),
            ControleId = ctrl ?? Guid.NewGuid(),
            ChannelId = chan ?? Guid.NewGuid(),
            ExecOrder = exec
        };

    private static ServiceControle MakeLink(Guid svc, Guid ctrl, Guid chan, int exec = 0) =>
        ServiceControle.Create(
            ServiceControleId.Of(Guid.NewGuid()),
            ServiceId.Of(svc),
            ControleId.Of(ctrl),
            ParamTypeId.Of(chan),
            exec);


    [Fact(DisplayName = "POST /api/serviceControles → 201 Created & Guid on valid request")]
    public async Task Post_ShouldReturn201_AndId_WhenValid()
    {
        // Arrange 
        var payload = ValidPayload();

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of((Guid)payload.GetType().GetProperty("ServiceId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Service>());

        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of((Guid)payload.GetType().GetProperty("ControleId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_fixture.Create<Controle>());

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of((Guid)payload.GetType().GetProperty("ChannelId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_fixture.Create<ParamType>());

        // Act
        var resp = await _client.PostAsJsonAsync("/api/serviceControles", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        id.Should().NotBeEmpty();

        _serviceControlRepoMock.Verify(r =>
            r.AddAsync(It.Is<ServiceControle>(sc =>
                    sc.ServiceId.Value == (Guid)payload.GetType().GetProperty("ServiceId")!.GetValue(payload)! &&
                    sc.ControleId.Value == (Guid)payload.GetType().GetProperty("ControleId")!.GetValue(payload)! &&
                    sc.ChannelId.Value == (Guid)payload.GetType().GetProperty("ChannelId")!.GetValue(payload)! &&
                    sc.ExecOrder == (int)payload.GetType().GetProperty("ExecOrder")!.GetValue(payload)!),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _serviceControlRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "POST /api/serviceControles → 400 when ServiceId is empty")]
    public async Task Post_ShouldReturn400_WhenServiceIdEmpty()
    {
        var bad = ValidPayload(svc: Guid.Empty);

        var resp = await _client.PostAsJsonAsync("/api/serviceControles", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("ServiceId")[0].GetString()
            .Should().Be("ServiceId must not be empty.");

        _serviceControlRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/serviceControles → 400 when ExecOrder < 0")]
    public async Task Post_ShouldReturn400_WhenExecOrderNegative()
    {
        var bad = ValidPayload(exec: -1);

        var resp = await _client.PostAsJsonAsync("/api/serviceControles", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("ExecOrder")[0].GetString()
            .Should().Be("ExecOrder must be greater than or equal to 0.");

        _serviceControlRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Theory(DisplayName = "POST /api/serviceControles → 404 when Service / Controle / Channel missing")]
    [InlineData("Service")]
    [InlineData("Controle")]
    [InlineData("Channel")]
    public async Task Post_ShouldReturn404_WhenReferenceMissing(string missing)
    {
        var payload = ValidPayload();

        // Only set up the *existing* ones
        if (missing != "Service")
            _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of((Guid)payload.GetType().GetProperty("ServiceId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_fixture.Create<Service>());

        if (missing != "Controle")
            _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of((Guid)payload.GetType().GetProperty("ControleId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(_fixture.Create<Controle>());

        if (missing != "Channel")
            _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of((Guid)payload.GetType().GetProperty("ChannelId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(_fixture.Create<ParamType>());

        var resp = await _client.PostAsJsonAsync("/api/serviceControles", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _serviceControlRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "POST /api/serviceControles → 409 when (Service,Controle,Channel) tuple already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateTuple()
    {
        var svc = Guid.NewGuid();
        var ctrl = Guid.NewGuid();
        var chan = Guid.NewGuid();

        _serviceRepoMock.Setup(r => r.GetByIdAsync(ServiceId.Of(svc), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<Service>());
        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(ctrl), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_fixture.Create<Controle>());
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chan), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_fixture.Create<ParamType>());

        _serviceControlRepoMock.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceControle, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(MakeLink(svc, ctrl, chan));

        var payload = ValidPayload(svc, ctrl, chan);

        var resp = await _client.PostAsJsonAsync("/api/serviceControles", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _serviceControlRepoMock.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}