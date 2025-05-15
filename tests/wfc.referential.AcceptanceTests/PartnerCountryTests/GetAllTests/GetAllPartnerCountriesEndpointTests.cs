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
using wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerCountryTests.GetAllTests;

public class GetAllPartnerCountriesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();

    public GetAllPartnerCountriesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var configured = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = configured.CreateClient();
    }

    private static PartnerCountry Pc(Guid? partner = null, Guid? country = null, bool enabled = true)
        => PartnerCountry.Create(
               PartnerCountryId.Of(Guid.NewGuid()),
               new PartnerId(partner ?? Guid.NewGuid()),
               new CountryId(country ?? Guid.NewGuid()),
               enabled);

    // simple DTO to deserialise paged result
    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);


    // 1) Happy-path paging
    [Fact(DisplayName = "GET /api/partnerCountries returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenPagingValid()
    {
        var rows = new[] { Pc(), Pc(), Pc() };

        _repoMock.Setup(r => r.GetAllPaginatedAsyncFiltered(
                            It.Is<GetAllPartnerCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(rows.Take(2).ToList());

        _repoMock.Setup(r => r.GetTotalCountAsync(
                            It.IsAny<GetAllPartnerCountriesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(rows.Length);

        var resp = await _client.GetAsync("/api/partnerCountries?pageNumber=1&pageSize=2");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);
        dto.TotalCount.Should().Be(3);
        dto.TotalPages.Should().Be(2);

        _repoMock.Verify(r => r.GetAllPaginatedAsyncFiltered(
                              It.Is<GetAllPartnerCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Filter by PartnerId
    [Fact(DisplayName = "GET /api/partnerCountries?partnerId={id} returns only rows of that partner")]
    public async Task Get_ShouldFilterByPartnerId()
    {
        var partnerId = Guid.NewGuid();
        var match = Pc(partnerId);
        var others = Pc();

        _repoMock.Setup(r => r.GetAllPaginatedAsyncFiltered(
                            It.Is<GetAllPartnerCountriesQuery>(q => q.PartnerId == partnerId),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PartnerCountry> { match });

        _repoMock.Setup(r => r.GetTotalCountAsync(
                            It.IsAny<GetAllPartnerCountriesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        var resp = await _client.GetAsync($"/api/partnerCountries?partnerId={partnerId}");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("partnerId").GetGuid().Should().Be(partnerId);

        _repoMock.Verify(r => r.GetAllPaginatedAsyncFiltered(
                              It.Is<GetAllPartnerCountriesQuery>(q => q.PartnerId == partnerId),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 3) Default paging when no query params
    [Fact(DisplayName = "GET /api/partnerCountries uses default paging (page=1, size=10)")]
    public async Task Get_ShouldUseDefaults_WhenNoPagingProvided()
    {
        var list = new[] { Pc(), Pc() };

        _repoMock.Setup(r => r.GetAllPaginatedAsyncFiltered(
                            It.Is<GetAllPartnerCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.ToList());

        _repoMock.Setup(r => r.GetTotalCountAsync(
                            It.IsAny<GetAllPartnerCountriesQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list.Length);

        var resp = await _client.GetAsync("/api/partnerCountries");
        var dto = await resp.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _repoMock.Verify(r => r.GetAllPaginatedAsyncFiltered(
                              It.Is<GetAllPartnerCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                              It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}