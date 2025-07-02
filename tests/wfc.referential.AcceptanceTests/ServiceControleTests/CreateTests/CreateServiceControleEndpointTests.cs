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
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceControleTests.CreateTests;

public class CreateServiceControleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private readonly Mock<IServiceControleRepository> _linkRepo = new();
    private readonly Mock<IServiceRepository> _svcRepo = new();
    private readonly Mock<IControleRepository> _ctrlRepo = new();
    private readonly Mock<IParamTypeRepository> _chanRepo = new();

    public CreateServiceControleEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IServiceControleRepository>();
                s.RemoveAll<IServiceRepository>();
                s.RemoveAll<IControleRepository>();
                s.RemoveAll<IParamTypeRepository>();
                s.RemoveAll<ICacheService>();

                _linkRepo.Setup(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((ServiceControle sc, CancellationToken _) => sc);

                _linkRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_linkRepo.Object);
                s.AddSingleton(_svcRepo.Object);
                s.AddSingleton(_ctrlRepo.Object);
                s.AddSingleton(_chanRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


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

        _svcRepo.Setup(r => r.GetByIdAsync(ServiceId.Of((Guid)payload.GetType().GetProperty("ServiceId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Service)) as Service);

        _ctrlRepo.Setup(r => r.GetByIdAsync(ControleId.Of((Guid)payload.GetType().GetProperty("ControleId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Controle)) as Controle);

        _chanRepo.Setup(r => r.GetByIdAsync(ParamTypeId.Of((Guid)payload.GetType().GetProperty("ChannelId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(ParamType)) as ParamType);

        // Act
        var resp = await _client.PostAsJsonAsync("/api/serviceControles", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        id.Should().NotBeEmpty();

        _linkRepo.Verify(r =>
            r.AddAsync(It.Is<ServiceControle>(sc =>
                    sc.ServiceId.Value == (Guid)payload.GetType().GetProperty("ServiceId")!.GetValue(payload)! &&
                    sc.ControleId.Value == (Guid)payload.GetType().GetProperty("ControleId")!.GetValue(payload)! &&
                    sc.ChannelId.Value == (Guid)payload.GetType().GetProperty("ChannelId")!.GetValue(payload)! &&
                    sc.ExecOrder == (int)payload.GetType().GetProperty("ExecOrder")!.GetValue(payload)!),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _linkRepo.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
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

        _linkRepo.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
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
            _svcRepo.Setup(r => r.GetByIdAsync(ServiceId.Of((Guid)payload.GetType().GetProperty("ServiceId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Service)) as Service);

        if (missing != "Controle")
            _ctrlRepo.Setup(r => r.GetByIdAsync(ControleId.Of((Guid)payload.GetType().GetProperty("ControleId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Controle)) as Controle);

        if (missing != "Channel")
            _chanRepo.Setup(r => r.GetByIdAsync(ParamTypeId.Of((Guid)payload.GetType().GetProperty("ChannelId")!.GetValue(payload)!), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(ParamType)) as ParamType);

        var resp = await _client.PostAsJsonAsync("/api/serviceControles", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _linkRepo.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "POST /api/serviceControles → 409 when (Service,Controle,Channel) tuple already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateTuple()
    {
        var svc = Guid.NewGuid();
        var ctrl = Guid.NewGuid();
        var chan = Guid.NewGuid();

        _svcRepo.Setup(r => r.GetByIdAsync(ServiceId.Of(svc), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Service)) as Service);
        _ctrlRepo.Setup(r => r.GetByIdAsync(ControleId.Of(ctrl), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Controle)) as Controle);
        _chanRepo.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chan), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(ParamType)) as ParamType);

        _linkRepo.Setup(r => r.GetOneByConditionAsync(
                            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceControle, bool>>>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(MakeLink(svc, ctrl, chan));

        var payload = ValidPayload(svc, ctrl, chan);

        var resp = await _client.PostAsJsonAsync("/api/serviceControles", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _linkRepo.Verify(r => r.AddAsync(It.IsAny<ServiceControle>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}