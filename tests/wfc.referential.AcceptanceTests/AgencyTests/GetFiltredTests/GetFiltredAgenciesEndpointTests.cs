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
using wfc.referential.Application.Agencies.Queries.GetFiltredAgencies;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.GetFiltredTests;

public class GetFiltredAgenciesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();

    public GetFiltredAgenciesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgencyRepository>();
                services.AddSingleton(_repoMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }


    /// <summary>Quickly builds a dummy <see cref="Agency"/> aggregate.</summary>
    private static Agency CreateAgency(string code, string name)
    {
        return Agency.Create(
            AgencyId.Of(Guid.NewGuid()),
            code,
            name,
            $"{code}-abbr",
            "42, Main St.",
            null,
            "+212600000000",
            "0522334455",
            "Sheet-A",
            "Acc-001",
            "90001",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            CityId.Of(Guid.NewGuid()),   
            null,
            null,
            null,
            null,
            null,
            null);
    }

    /// <summary>Lightweight DTO for reading the endpoint response.</summary>
    private record PagedResultDto<T>(T[] Items,
                                     int PageNumber,
                                     int PageSize,
                                     int TotalCount,
                                     int TotalPages);


    [Fact(DisplayName = "GET /api/agencies returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var all = new[]
        {
            CreateAgency("AG-001", "Agency One"),
            CreateAgency("AG-002", "Agency Two"),
            CreateAgency("AG-003", "Agency Three"),
            CreateAgency("AG-004", "Agency Four"),
            CreateAgency("AG-005", "Agency Five")
        };

        var paged = new PagedResult<Agency>(
            all.Take(2).ToList(),
            totalCount: all.Length,
            pageNumber: 1,
            pageSize: 2);

        _repoMock
            .Setup(r => r.GetPagedByCriteriaAsync(
                It.Is<GetFiltredAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        // Act
        var response = await _client.GetAsync("/api/agencies?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                              It.Is<GetFiltredAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                              1, 2, It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencies?code=AG-003 returns only the matching agency")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var ag3 = CreateAgency("AG-003", "Agency Three");

        var paged = new PagedResult<Agency>(
            new List<Agency> { ag3 },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10);

        _repoMock
            .Setup(r => r.GetPagedByCriteriaAsync(
                It.Is<GetFiltredAgenciesQuery>(q => q.Code == "AG-003"),
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        // Act
        var response = await _client.GetAsync("/api/agencies?code=AG-003");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("AG-003");

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                              It.Is<GetFiltredAgenciesQuery>(q => q.Code == "AG-003"),
                              1, 10, It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/agencies uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        var agencies = new[]
        {
            CreateAgency("AG-001", "Agency One"),
            CreateAgency("AG-002", "Agency Two"),
            CreateAgency("AG-003", "Agency Three")
        };

        var paged = new PagedResult<Agency>(
            agencies.ToList(),
            totalCount: agencies.Length,
            pageNumber: 1,
            pageSize: 10); 

        _repoMock
            .Setup(r => r.GetPagedByCriteriaAsync(
                It.Is<GetFiltredAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        // Act
        var response = await _client.GetAsync("/api/agencies");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                              It.Is<GetFiltredAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                              1, 10, It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}