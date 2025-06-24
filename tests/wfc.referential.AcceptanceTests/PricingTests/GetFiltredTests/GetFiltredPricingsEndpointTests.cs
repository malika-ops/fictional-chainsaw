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
using wfc.referential.Application.Pricings.Queries.GetFiltredPricings;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PricingTests.GetFiltredTests;

public class GetFiltredPricingsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPricingRepository> _repoMock = new();

    public GetFiltredPricingsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPricingRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    private static Pricing CreatePricing(
        string code,
        string channel = "Branch",
        decimal min = 10,
        decimal max = 100,
        decimal? fixedFee = 5,
        decimal? rate = null,
        bool enabled = true)
    {
        var p = Pricing.Create(
            PricingId.Of(Guid.NewGuid()),
            code,
            channel,
            min,
            max,
            fixedFee,
            rate,
            CorridorId.Of(Guid.NewGuid()),
            ServiceId.Of(Guid.NewGuid()),
            AffiliateId.Of(Guid.NewGuid()));
        if (!enabled) p.Disable();
        return p;
    }

    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);


    [Fact(DisplayName = "GET /api/pricings returns paged list")]
    public async Task Get_ShouldReturnPagedList()
    {
        var items = new[]
        {
                CreatePricing("C1"),
                CreatePricing("C2")
            };

        var paged = new PagedResult<Pricing>(items.ToList(), 5, 1, 2);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/pricings?pageNumber=1&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalPages.Should().Be(3);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/pricings?code=ABC filters by Code")]
    public async Task Get_ShouldFilterByCode()
    {
        const string code = "ABC";
        var pricing = CreatePricing(code);
        var paged = new PagedResult<Pricing>(new List<Pricing> { pricing }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q => q.Code == code),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/pricings?code={code}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items[0].GetProperty("code").GetString().Should().Be(code);
    }

    [Fact(DisplayName = "GET /api/pricings?channel=Online filters by Channel")]
    public async Task Get_ShouldFilterByChannel()
    {
        const string channel = "Online";
        var pricing = CreatePricing("C1", channel);
        var paged = new PagedResult<Pricing>(new List<Pricing> { pricing }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q => q.Channel == channel),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/pricings?channel={channel}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("channel").GetString().Should().Be(channel);
    }

    [Fact(DisplayName = "GET /api/pricings?isEnabled=false filters disabled records")]
    public async Task Get_ShouldFilterByEnabled()
    {
        var disabled = CreatePricing("X", enabled: false);
        var paged = new PagedResult<Pricing>(new List<Pricing> { disabled }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q => q.IsEnabled == false),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/pricings?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/pricings default paging when no params")]
    public async Task Get_ShouldUseDefaults()
    {
        var list = new[]
        {
                CreatePricing("A"), CreatePricing("B"), CreatePricing("C")
            };
        var paged = new PagedResult<Pricing>(list.ToList(), 3, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                      It.Is<GetFiltredPricingsQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                      1,
                      10,
                      It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>())) 
           .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/pricings");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "GET /api/pricings with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        var code = "MULTI";
        var chan = "Online";
        var corrId = Guid.NewGuid();
        var svcId = Guid.NewGuid();

        var pricing = CreatePricing(code, chan);
        var paged = new PagedResult<Pricing>(new List<Pricing> { pricing }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q =>
                                q.Code == code &&
                                q.Channel == chan &&
                                q.CorridorId == corrId &&
                                q.ServiceId == svcId &&
                                q.IsEnabled == true),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/pricings?code={code}&channel={chan}&corridorId={corrId}&serviceId={svcId}&isEnabled=true");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GET /api/pricings returns empty list when no match")]
    public async Task Get_ShouldReturnEmpty_WhenNoMatch()
    {
        var paged = new PagedResult<Pricing>(new List<Pricing>(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPricingsQuery>(q => q.Code == "NONE"),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Pricing, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/pricings?code=NONE");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}