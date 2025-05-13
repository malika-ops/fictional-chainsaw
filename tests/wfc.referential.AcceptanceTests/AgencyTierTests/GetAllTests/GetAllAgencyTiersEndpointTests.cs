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
using wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.GetAllTests;

public class GetAllAgencyTiersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _repoMock = new();

    public GetAllAgencyTiersEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var configured = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyTierRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = configured.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static AgencyTier At(Guid agencyId, Guid tierId, string code, bool enabled = true)
    {
        return AgencyTier.Create(
            AgencyTierId.Of(Guid.NewGuid()),
            new AgencyId(agencyId),
            new TierId(tierId),
            code,
            password: string.Empty,
            isEnabled: enabled);
    }

    private record PagedDto<T>(
        T[] Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages);

    /* ---------- tests ---------- */

    // 1) Happy-path paging ---------------------------------------------------
    [Fact(DisplayName = "GET /api/agencyTiers returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenPagingValid()
    {
        var agency1 = Guid.NewGuid();
        var agency2 = Guid.NewGuid();
        var tier1 = Guid.NewGuid();
        var tier2 = Guid.NewGuid();

        var all = new[]
        {
            At(agency1, tier1, "C1"),
            At(agency2, tier2, "C2"),
            At(agency1, tier2, "C3")
        };

        _repoMock.Setup(r => r.GetFilteredAgencyTiersAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(all.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllAgencyTiersQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(all.Length);

        var resp = await _client.GetAsync("/api/agencyTiers?pageNumber=1&pageSize=2");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(3);
        dto.TotalPages.Should().Be(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredAgencyTiersAsync(
                              It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Filter by Code ------------------------------------------------------
    [Fact(DisplayName = "GET /api/agencyTiers?code=C2 returns only mapping with code C2")]
    public async Task Get_ShouldFilterByCode()
    {
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var c2 = At(agencyId, tierId, "C2");

        _repoMock.Setup(r => r.GetFilteredAgencyTiersAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.Code == "C2"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<AgencyTier> { c2 });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllAgencyTiersQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var resp = await _client.GetAsync("/api/agencyTiers?code=C2");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("C2");

        _repoMock.Verify(r => r.GetFilteredAgencyTiersAsync(
                              It.Is<GetAllAgencyTiersQuery>(q => q.Code == "C2"),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 3) Default paging ------------------------------------------------------
    [Fact(DisplayName = "GET /api/agencyTiers uses default paging (page=1, size=10)")]
    public async Task Get_ShouldUseDefaults_WhenNoPagingProvided()
    {
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var list = new[] { At(agencyId, tierId, "C1"),
                           At(agencyId, tierId, "C2") };

        _repoMock.Setup(r => r.GetFilteredAgencyTiersAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllAgencyTiersQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.Length);

        var resp = await _client.GetAsync("/api/agencyTiers");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _repoMock.Verify(r => r.GetFilteredAgencyTiersAsync(
                              It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}
