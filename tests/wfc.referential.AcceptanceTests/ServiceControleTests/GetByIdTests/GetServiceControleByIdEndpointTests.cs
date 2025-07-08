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
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceControleTests.GetByIdTests;

public class GetServiceControleByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static ServiceControle Make(Guid id,
                                        Guid serviceId,
                                        Guid controleId,
                                        Guid channelId,
                                        int execOrder = 0,
                                        bool enabled = true)
    {
        var link = ServiceControle.Create(
            ServiceControleId.Of(id),
            ServiceId.Of(serviceId),
            ControleId.Of(controleId),
            ParamTypeId.Of(channelId),
            execOrder);

        if (!enabled) link.Disable();
        return link;
    }

    private record LinkDto(Guid Id,
                           Guid ServiceId,
                           Guid ControleId,
                           int ExecOrder,
                           bool IsEnabled);



    [Fact(DisplayName = "GET /api/serviceControles/{id} → 200 when link exists")]
    public async Task Get_ShouldReturn200_WhenLinkExists()
    {
        var id = Guid.NewGuid();
        var svc = Guid.NewGuid();
        var ctl = Guid.NewGuid();
        var ch = Guid.NewGuid();

        var entity = Make(id, svc, ctl, ch, execOrder: 3);

        _serviceControlRepoMock.Setup(r => r.GetByIdWithIncludesAsync(
                        ServiceControleId.Of(id),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/serviceControles/{id}");
        var dto = await res.Content.ReadFromJsonAsync<LinkDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Id.Should().Be(id);
        dto.ServiceId.Should().Be(svc);
        dto.ControleId.Should().Be(ctl);
        dto.ExecOrder.Should().Be(3);
        dto.IsEnabled.Should().BeTrue();

        _serviceControlRepoMock.Verify(r => r.GetByIdWithIncludesAsync(
                        ServiceControleId.Of(id),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<ServiceControle, object>>[]>()),
                     Times.Once);
    }



    [Fact(DisplayName = "GET /api/serviceControles/{id} → 200 for disabled link")]
    public async Task Get_ShouldReturn200_WhenLinkDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id,
                          Guid.NewGuid(),
                          Guid.NewGuid(),
                          Guid.NewGuid(),
                          enabled: false);

        _serviceControlRepoMock.Setup(r => r.GetByIdWithIncludesAsync(
                        ServiceControleId.Of(id),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/serviceControles/{id}");
        var dto = await res.Content.ReadFromJsonAsync<LinkDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }


    [Fact(DisplayName = "GET /api/serviceControles/{id} → 404 when link not found")]
    public async Task Get_ShouldReturn404_WhenLinkNotFound()
    {
        var missing = Guid.NewGuid();

        _serviceControlRepoMock.Setup(r => r.GetByIdWithIncludesAsync(
                        ServiceControleId.Of(missing),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync((ServiceControle?)null);

        var res = await _client.GetAsync($"/api/serviceControles/{missing}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _serviceControlRepoMock.Verify(r => r.GetByIdWithIncludesAsync(
                        ServiceControleId.Of(missing),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<ServiceControle, object>>[]>()),
                     Times.Once);
    }


    [Fact(DisplayName = "GET /api/serviceControles/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/serviceControles/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _serviceControlRepoMock.Verify(r => r.GetByIdWithIncludesAsync(
                        It.IsAny<ServiceControleId>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<Expression<Func<ServiceControle, object>>[]>()),
                     Times.Never);
    }
}
