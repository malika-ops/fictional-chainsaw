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
using wfc.referential.Application.Tiers.Queries.GetAllTiers;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.GetAllTests;

public class GetAllTiersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _repoMock = new();

    public GetAllTiersEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var configured = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ITierRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = configured.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static Tier Tr(string name, string desc = "descr", bool enabled = true)
        => Tier.Create(new TierId(Guid.NewGuid()), name, desc, enabled);

    // simple DTO to deserialise paged result
    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);

    /* ----------------- tests ----------------- */

    // 1) Happy-path paging
    [Fact(DisplayName = "GET /api/tiers returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenPagingValid()
    {
        var tiers = new[] { Tr("Standard"), Tr("Premium"), Tr("VIP") };

        _repoMock.Setup(r => r.GetFilteredTiersAsync(
                            It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(tiers.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTiersQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(tiers.Length);

        var resp = await _client.GetAsync("/api/tiers?pageNumber=1&pageSize=2");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);
        dto.TotalCount.Should().Be(3);
        dto.TotalPages.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredTiersAsync(
                              It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Filter by Name
    [Fact(DisplayName = "GET /api/tiers?name=Premium returns only Premium tier")]
    public async Task Get_ShouldFilterByName()
    {
        var premium = Tr("Premium");

        _repoMock.Setup(r => r.GetFilteredTiersAsync(
                            It.Is<GetAllTiersQuery>(q => q.Name == "Premium"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Tier> { premium });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTiersQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var resp = await _client.GetAsync("/api/tiers?name=Premium");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("name").GetString().Should().Be("Premium");

        _repoMock.Verify(r => r.GetFilteredTiersAsync(
                              It.Is<GetAllTiersQuery>(q => q.Name == "Premium"),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 3) Default paging when no query params
    [Fact(DisplayName = "GET /api/tiers uses default paging (page=1, size=10)")]
    public async Task Get_ShouldUseDefaults_WhenNoPagingProvided()
    {
        var list = new[] { Tr("Standard"), Tr("Premium") };

        _repoMock.Setup(r => r.GetFilteredTiersAsync(
                            It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTiersQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.Length);

        var resp = await _client.GetAsync("/api/tiers");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _repoMock.Verify(r => r.GetFilteredTiersAsync(
                              It.Is<GetAllTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}
