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
using wfc.referential.Application.Countries.Queries.GetAllCounties;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.GetAllTests;

public class GetAllCountriesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _repoMock = new();

    // ───────────────────────── constructor ─────────────────────────
    public GetAllCountriesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to build dummy countries quickly
    private static Country Ctry(string code, string name) =>
        Country.Create(
            CountryId.Of(Guid.NewGuid()),
            code[..2],                // Abbrev
            name,
            code,
            code[..2],
            code,
            "+0",
            "UTC",
            false,
            false,
            2,
            true,
            MonetaryZoneId.Of(Guid.NewGuid()),
            null);

    // DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    // ────────────────────────────────────────────────────────────────
    // 1) Happy‑path explicit paging
    // ────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "GET /api/countries returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var all = new[] { Ctry("USA", "United States"), Ctry("CAN", "Canada"),
                          Ctry("MEX", "Mexico"),        Ctry("BRA", "Brazil") };

        _repoMock.Setup(r => r.GetAllCountriesPaginatedAsyncFiltred(
                            It.Is<GetAllCountriesQuery>(q => q.PageNumber == 2 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(all.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllCountriesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(all.Length);

        // Act
        var response = await _client.GetAsync("/api/countries?pageNumber=2&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(4);
        dto.TotalPages.Should().Be(2);
        dto.PageNumber.Should().Be(2);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetAllCountriesPaginatedAsyncFiltred(
                                It.Is<GetAllCountriesQuery>(q => q.PageNumber == 2 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // 2) Filter by code
    // ────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "GET /api/countries?code=USA returns only USA")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var usa = Ctry("USA", "United States");

        _repoMock.Setup(r => r.GetAllCountriesPaginatedAsyncFiltred(
                            It.Is<GetAllCountriesQuery>(q => q.Code == "USA"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Country> { usa });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllCountriesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/countries?code=USA");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("USA");

        _repoMock.Verify(r => r.GetAllCountriesPaginatedAsyncFiltred(
                                It.Is<GetAllCountriesQuery>(q => q.Code == "USA"),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // 3) Default paging (page=1, size=10)
    // ────────────────────────────────────────────────────────────────
    [Fact(DisplayName = "GET /api/countries uses default paging when no params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        var list = new[] { Ctry("USA", "United States"),
                           Ctry("CAN", "Canada") };

        _repoMock.Setup(r => r.GetAllCountriesPaginatedAsyncFiltred(
                            It.Is<GetAllCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllCountriesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.Length);

        // Act
        var response = await _client.GetAsync("/api/countries");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _repoMock.Verify(r => r.GetAllCountriesPaginatedAsyncFiltred(
                                It.Is<GetAllCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}