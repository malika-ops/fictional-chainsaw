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
using wfc.referential.Application.Agencies.Queries.GetAllAgencies;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.GetAllTests;

public class GetAllAgenciesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();

    public GetAllAgenciesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var configured = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = configured.CreateClient();
    }

    // helper → build an Agency quickly
    private static Agency Ag(string code, string name)
    {
        var abbr = code.Length >= 3 ? code[..3] : code;
        return Agency.Create(
            AgencyId.Of(Guid.NewGuid()),
            code, name, abbr,
            "1 Main St", null,
            "0600000000", "", "sheet", "401122",
            "", "", "10000", "",
            null, null,
            true,
            null, null, null, null, null);
    }
    // simple DTO to de-serialise the endpoint’s paged result
    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);

    // Happy-path paging
    [Fact(DisplayName = "GET /api/agencies returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenPagingValid()
    {
        var all = new[] { Ag("A1", "Alpha"), Ag("B2", "Beta"),
                          Ag("C3", "Charlie") };

        _repoMock.Setup(r => r.GetAllAgenciesPaginatedAsyncFiltered(
                            It.Is<GetAllAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(all.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllAgenciesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(all.Length);

        var resp = await _client.GetAsync("/api/agencies?pageNumber=1&pageSize=2");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(3);
        dto.TotalPages.Should().Be(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetAllAgenciesPaginatedAsyncFiltered(
                              It.Is<GetAllAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

     // Filter by Code
    [Fact(DisplayName = "GET /api/agencies?code=A1 returns only agency A1")]
    public async Task Get_ShouldFilterByCode()
    {
        var a1 = Ag("A1", "Alpha");

        _repoMock.Setup(r => r.GetAllAgenciesPaginatedAsyncFiltered(
                            It.Is<GetAllAgenciesQuery>(q => q.Code == "A1"),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Agency> { a1 });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllAgenciesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var resp = await _client.GetAsync("/api/agencies?code=A1");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("A1");

        _repoMock.Verify(r => r.GetAllAgenciesPaginatedAsyncFiltered(
                              It.Is<GetAllAgenciesQuery>(q => q.Code == "A1"),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

     // Default paging when no params supplied
    [Fact(DisplayName = "GET /api/agencies uses default paging (page=1, size=10)")]
    public async Task Get_ShouldUseDefaults_WhenNoPagingProvided()
    {
        var list = new[] { Ag("A1", "Alpha"), Ag("B2", "Beta") };

        _repoMock.Setup(r => r.GetAllAgenciesPaginatedAsyncFiltered(
                            It.Is<GetAllAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllAgenciesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.Length);

        var resp = await _client.GetAsync("/api/agencies");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _repoMock.Verify(r => r.GetAllAgenciesPaginatedAsyncFiltered(
                              It.Is<GetAllAgenciesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                              It.IsAny<CancellationToken>()),
         Times.Once);
    }
}