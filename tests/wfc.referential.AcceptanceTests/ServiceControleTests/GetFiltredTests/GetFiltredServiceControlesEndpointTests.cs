using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
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
using wfc.referential.Application.ServiceControles.Queries.GetFiltredServiceControles;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceControleTests.GetFiltredTests;

public class GetFiltredServiceControlesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IServiceControleRepository> _repo = new();

    public GetFiltredServiceControlesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IServiceControleRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static ServiceControle Make(Guid serviceId,
                                        Guid controleId,
                                        Guid channelId,
                                        int execOrder = 0,
                                        bool enabled = true)
    {
        var link = ServiceControle.Create(
            ServiceControleId.Of(Guid.NewGuid()),
            ServiceId.Of(serviceId),
            ControleId.Of(controleId),
            ParamTypeId.Of(channelId),
            execOrder);

        if (!enabled) link.Disable();
        return link;
    }

    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);


    [Fact(DisplayName = "GET /api/serviceControles returns paged list")]
    public async Task Get_ShouldReturnPagedList()
    {
        var sId1 = Guid.NewGuid(); var cId1 = Guid.NewGuid(); var ch1 = Guid.NewGuid();
        var sId2 = Guid.NewGuid(); var cId2 = Guid.NewGuid(); var ch2 = Guid.NewGuid();

        var item1 = Make(sId1, cId1, ch1, execOrder: 1);
        var item2 = Make(sId2, cId2, ch2, execOrder: 2);

        var page = new PagedResult<ServiceControle>(new() { item1, item2 }, 5, 1, 2);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                        1, 2, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(page);

        var res = await _client.GetAsync("/api/serviceControles?pageNumber=1&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        dto.Items[0].GetProperty("serviceId").GetGuid().Should().Be(sId1);
        dto.Items[0].GetProperty("controleId").GetGuid().Should().Be(cId1);
        dto.Items[0].GetProperty("execOrder").GetInt32().Should().Be(1);

        dto.Items[1].GetProperty("serviceId").GetGuid().Should().Be(sId2);
        dto.Items[1].GetProperty("controleId").GetGuid().Should().Be(cId2);
        dto.Items[1].GetProperty("execOrder").GetInt32().Should().Be(2);

        _repo.Verify(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                        1, 2, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/serviceControles?serviceId filters by ServiceId")]
    public async Task Get_ShouldFilterByServiceId()
    {
        var svcId = Guid.NewGuid();
        var entity = Make(svcId, Guid.NewGuid(), Guid.NewGuid());

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.ServiceId == svcId),
                        1, 10, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(new PagedResult<ServiceControle>(new() { entity }, 1, 1, 10));

        var res = await _client.GetAsync($"/api/serviceControles?serviceId={svcId}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("serviceId").GetGuid().Should().Be(svcId);
    }

    [Fact(DisplayName = "GET /api/serviceControles?controleId filters by ControleId")]
    public async Task Get_ShouldFilterByControleId()
    {
        var ctlId = Guid.NewGuid();
        var entity = Make(Guid.NewGuid(), ctlId, Guid.NewGuid());

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.ControleId == ctlId),
                        1, 10, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(new PagedResult<ServiceControle>(new() { entity }, 1, 1, 10));

        var res = await _client.GetAsync($"/api/serviceControles?controleId={ctlId}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("controleId").GetGuid().Should().Be(ctlId);
    }


    [Fact(DisplayName = "GET /api/serviceControles?isEnabled=false filters disabled")]
    public async Task Get_ShouldFilterByDisabled()
    {
        var disabled = Make(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), enabled: false);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.IsEnabled == false),
                        1, 10, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(new PagedResult<ServiceControle>(new() { disabled }, 1, 1, 10));

        var res = await _client.GetAsync("/api/serviceControles?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }


    [Fact(DisplayName = "GET /api/serviceControles uses defaults when no params")]
    public async Task Get_ShouldUseDefaultPaging()
    {
        var links = new[]
        {
            Make(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            Make(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            Make(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
        };

        var page = new PagedResult<ServiceControle>(links.ToList(), 3, 1, 10);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                        1, 10, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(page);

        var res = await _client.GetAsync("/api/serviceControles");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);
    }


    [Fact(DisplayName = "GET /api/serviceControles with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        var svcId = Guid.NewGuid();
        var ctlId = Guid.NewGuid();
        var chId = Guid.NewGuid();
        var link = Make(svcId, ctlId, chId);

        var page = new PagedResult<ServiceControle>(new() { link }, 1, 1, 10);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q =>
                            q.ServiceId == svcId &&
                            q.ControleId == ctlId &&
                            q.ChannelId == chId &&
                            q.IsEnabled == true),
                        1, 10, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(page);

        var url = $"/api/serviceControles?serviceId={svcId}&controleId={ctlId}&channelId={chId}&isEnabled=true";
        var res = await _client.GetAsync(url);
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("serviceId").GetGuid().Should().Be(svcId);
    }



    [Fact(DisplayName = "GET /api/serviceControles returns empty list when no match")]
    public async Task Get_ShouldReturnEmpty_WhenNoMatch()
    {
        var page = new PagedResult<ServiceControle>(new(), 0, 1, 10);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredServiceControlesQuery>(q => q.ServiceId == Guid.Empty),
                        1, 10, It.IsAny<CancellationToken>(),
                 It.IsAny<Expression<Func<ServiceControle, object>>[]>()))
             .ReturnsAsync(page);

        var res = await _client.GetAsync("/api/serviceControles?serviceId=00000000-0000-0000-0000-000000000000");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}