using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceControleTests.PatchTests;

public class PatchServiceControleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private readonly Mock<IServiceControleRepository> _linkRepo = new();
    private readonly Mock<IServiceRepository> _svcRepo = new();
    private readonly Mock<IControleRepository> _ctlRepo = new();
    private readonly Mock<IParamTypeRepository> _chnRepo = new();

    public PatchServiceControleEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IServiceControleRepository>();
                s.RemoveAll<IServiceRepository>();
                s.RemoveAll<IControleRepository>();
                s.RemoveAll<IParamTypeRepository>();
                s.RemoveAll<ICacheService>();

                _linkRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_linkRepo.Object);
                s.AddSingleton(_svcRepo.Object);
                s.AddSingleton(_ctlRepo.Object);
                s.AddSingleton(_chnRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }


    private static ServiceControle Make(Guid id, Guid svc, Guid ctl,
                                        Guid chn, int exec = 0, bool en = true)
    {
        var sc = ServiceControle.Create(
            ServiceControleId.Of(id),
            ServiceId.Of(svc),
            ControleId.Of(ctl),
            ParamTypeId.Of(chn),
            exec);

        if (!en) sc.Disable();
        return sc;
    }

    private static async Task<HttpResponseMessage> PatchJsonAsync(HttpClient c, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await c.SendAsync(req);
    }

    private static async Task<bool> ReadBool(HttpResponseMessage r) =>
        (await r.Content.ReadFromJsonAsync<bool>());


    [Fact(DisplayName = "PATCH → 200 when changing only ExecOrder")]
    public async Task Patch_ShouldReturn200_WhenExecOrderChanged()
    {
        var id = Guid.NewGuid();
        var svc = Guid.NewGuid();
        var ctl = Guid.NewGuid();
        var chn = Guid.NewGuid();

        var link = Make(id, svc, ctl, chn, exec: 1);
        _linkRepo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(link);

        var payload = new { ServiceControleId = id, ExecOrder = 9 };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{id}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        link.ExecOrder.Should().Be(9);
        link.IsEnabled.Should().BeTrue();
        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH → 200 when disabling link")]
    public async Task Patch_ShouldReturn200_WhenIsEnabledChanged()
    {
        var id = Guid.NewGuid();
        var svc = Guid.NewGuid();
        var ctl = Guid.NewGuid();
        var chn = Guid.NewGuid();

        var link = Make(id, svc, ctl, chn, exec: 0, en: true);
        _linkRepo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(link);

        var payload = new { ServiceControleId = id, IsEnabled = false };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{id}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        link.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "PATCH keeps same tuple → 200")]
    public async Task Patch_ShouldReturn200_WhenKeepingSameTuple()
    {
        var id = Guid.NewGuid();
        var svc = Guid.NewGuid();
        var ctl = Guid.NewGuid();
        var chn = Guid.NewGuid();

        var link = Make(id, svc, ctl, chn);
        _linkRepo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(link);

        // uniqueness check returns same entity
        _linkRepo.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<ServiceControle, bool>>>(),
                                                      It.IsAny<CancellationToken>()))
                 .ReturnsAsync(link);

        var payload = new { ServiceControleId = id, ExecOrder = 7 };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{id}", payload);
        var ok = await ReadBool(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();
        link.ExecOrder.Should().Be(7);
    }


    [Fact(DisplayName = "PATCH → 400 when ServiceControleId is empty")]
    public async Task Patch_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new { ServiceControleId = Guid.Empty, ExecOrder = 1 };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{Guid.Empty}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH → 400 when ExecOrder negative")]
    public async Task Patch_ShouldReturn400_WhenExecOrderNegative()
    {
        var id = Guid.NewGuid();
        var payload = new { ServiceControleId = id, ExecOrder = -3 };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PATCH → 404 when ServiceControle not found")]
    public async Task Patch_ShouldReturn404_WhenLinkMissing()
    {
        var id = Guid.NewGuid();
        _linkRepo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ServiceControle?)null);

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{id}", new { ServiceControleId = id });
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PATCH → 409 when tuple duplicates another row")]
    public async Task Patch_ShouldReturn409_WhenDuplicateTuple()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var svc2 = Guid.NewGuid();
        var ctl2 = Guid.NewGuid();
        var chn2 = Guid.NewGuid();

        var target = Make(id1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var duplicate = Make(id2, svc2, ctl2, chn2);

        _linkRepo.Setup(r => r.GetByIdAsync(ServiceControleId.Of(id1), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _svcRepo.Setup(r => r.GetByIdAsync(ServiceId.Of(svc2), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Service)) as Service);
        _ctlRepo.Setup(r => r.GetByIdAsync(ControleId.Of(ctl2), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Controle)) as Controle);
        _chnRepo.Setup(r => r.GetByIdAsync(ParamTypeId.Of(chn2), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(ParamType)) as ParamType);

        _linkRepo.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<ServiceControle, bool>>>(),
                                                      It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicate);

        var payload = new { ServiceControleId = id1, ServiceId = svc2, ControleId = ctl2, ChannelId = chn2 };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{id1}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PATCH → 404 when route id malformed")]
    public async Task Patch_ShouldReturn404_WhenRouteMalformed()
    {
        const string bad = "not-a-guid";
        var payload = new { ServiceControleId = Guid.NewGuid(), ExecOrder = 1 };

        var res = await PatchJsonAsync(_client, $"/api/serviceControles/{bad}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _linkRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}