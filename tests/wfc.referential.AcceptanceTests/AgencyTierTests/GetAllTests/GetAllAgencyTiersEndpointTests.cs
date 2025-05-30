using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
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

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgencyTierRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static AgencyTier CreateAgencyTier(Guid agencyId, Guid tierId, string code, bool enabled = true)
    {
        var entity = AgencyTier.Create(
            AgencyTierId.Of(Guid.NewGuid()),
            AgencyId.Of(agencyId),
            TierId.Of(tierId),
            code,
            null);

        if (!enabled) entity.Disable();
        return entity;
    }

    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/agencyTiers returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var items = new[]
        {
                CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), "C-001"),
                CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), "C-002")
            };

        var paged = new PagedResult<AgencyTier>(
            items.ToList(),
            totalCount: 5,
            pageNumber: 1,
            pageSize: 2);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1,
                            2,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        // Act
        var res = await _client.GetAsync("/api/agencyTiers?pageNumber=1&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1,
                                2,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencyTiers?agencyId={id} filters by Agency")]
    public async Task Get_ShouldFilterByAgencyId()
    {
        var agencyId = Guid.NewGuid();

        var entity = CreateAgencyTier(agencyId, Guid.NewGuid(), "AG-1");
        var paged = new PagedResult<AgencyTier>(new List<AgencyTier> { entity }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.AgencyId == agencyId),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/agencyTiers?agencyId={agencyId}");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("agencyId").GetGuid().Should().Be(agencyId);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllAgencyTiersQuery>(q => q.AgencyId == agencyId),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencyTiers?tierId={id} filters by Tier")]
    public async Task Get_ShouldFilterByTierId()
    {
        var tierId = Guid.NewGuid();
        var entity = CreateAgencyTier(Guid.NewGuid(), tierId, "T-1");
        var paged = new PagedResult<AgencyTier>(new List<AgencyTier> { entity }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.TierId == tierId),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/agencyTiers?tierId={tierId}");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("tierId").GetGuid().Should().Be(tierId);
    }

    [Fact(DisplayName = "GET /api/agencyTiers?code=X123 filters by Code")]
    public async Task Get_ShouldFilterByCode()
    {
        const string code = "X123";
        var entity = CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), code);
        var paged = new PagedResult<AgencyTier>(new List<AgencyTier> { entity }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.Code == code),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/agencyTiers?code={code}");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be(code);
    }

    [Fact(DisplayName = "GET /api/agencyTiers?isEnabled=false filters disabled links")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        var disabled = CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), "DIS", enabled: false);
        var paged = new PagedResult<AgencyTier>(new List<AgencyTier> { disabled }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.IsEnabled == false),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/agencyTiers?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/agencyTiers uses defaults when no params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        var entities = new[]
        {
                CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), "C-1"),
                CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), "C-2"),
                CreateAgencyTier(Guid.NewGuid(), Guid.NewGuid(), "C-3")
            };

        var paged = new PagedResult<AgencyTier>(entities.ToList(), 3, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/agencyTiers");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "GET /api/agencyTiers with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        const string code = "MULTI";

        var entity = CreateAgencyTier(agencyId, tierId, code);
        var paged = new PagedResult<AgencyTier>(new List<AgencyTier> { entity }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q =>
                                q.AgencyId == agencyId &&
                                q.TierId == tierId &&
                                q.Code == code &&
                                q.IsEnabled == true),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/agencyTiers?agencyId={agencyId}&tierId={tierId}&code={code}&isEnabled=true");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be(code);
    }

    [Fact(DisplayName = "GET /api/agencyTiers returns empty list when no match")]
    public async Task Get_ShouldReturnEmptyList_WhenNoMatch()
    {
        var paged = new PagedResult<AgencyTier>(new List<AgencyTier>(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllAgencyTiersQuery>(q => q.Code == "NONE"),
                            1,
                            10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/agencyTiers?code=NONE");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}