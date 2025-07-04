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
using wfc.referential.Application.PartnerCountries.Queries.GetFiltredPartnerCountries;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using Xunit;

using BuildingBlocks.Core.Pagination;

namespace wfc.referential.AcceptanceTests.PartnerCountryTests.GetFiltredTests;

public class GetFiltredPartnerCountriesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();

    public GetFiltredPartnerCountriesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
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

        _client = custom.CreateClient();
    }

    private static PartnerCountry Make(Guid partnerId, Guid countryId, bool enabled = true)
    {
        var pc = PartnerCountry.Create(
                    PartnerCountryId.Of(Guid.NewGuid()),
                    PartnerId.Of(partnerId),
                    CountryId.Of(countryId));

        if (!enabled) pc.Disable();
        return pc;
    }

    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);


    [Fact(DisplayName = "GET /api/partner-countries returns paged list")]
    public async Task Get_ShouldReturnPagedList()
    {
        // Arrange
        var items = new[]
        {
                Make(Guid.NewGuid(), Guid.NewGuid()),
                Make(Guid.NewGuid(), Guid.NewGuid())
            };

        var paged = new PagedResult<PartnerCountry>(
            items.ToList(),
            totalCount: 5,
            pageNumber: 2,
            pageSize: 2);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPartnerCountriesQuery>(q => q.PageNumber == 2 && q.PageSize == 2),
                            2,
                            2,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        // Act
        var res = await _client.GetAsync("/api/partner-countries?pageNumber=2&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.PageNumber.Should().Be(2);
        dto.PageSize.Should().Be(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);

        _repoMock.VerifyAll();
    }


    [Fact(DisplayName = "GET /api/partner-countries?partnerId= filters by Partner")]
    public async Task Get_ShouldFilterByPartnerId()
    {
        var partnerId = Guid.NewGuid();
        var pc = Make(partnerId, Guid.NewGuid());
        var paged = new PagedResult<PartnerCountry>(new() { pc }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<GetFiltredPartnerCountriesQuery>(),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/partner-countries?partnerId={partnerId}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("partnerId").GetGuid().Should().Be(partnerId);
    }


    [Fact(DisplayName = "GET /api/partner-countries?countryId= filters by Country")]
    public async Task Get_ShouldFilterByCountryId()
    {
        var countryId = Guid.NewGuid();
        var pc = Make(Guid.NewGuid(), countryId);
        var paged = new PagedResult<PartnerCountry>(new() { pc }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPartnerCountriesQuery>(q => q.CountryId == countryId),
                            1, 10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/partner-countries?countryId={countryId}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("countryId").GetGuid().Should().Be(countryId);
    }


    [Fact(DisplayName = "GET /api/partner-countries?isEnabled=false filters disabled")]
    public async Task Get_ShouldFilterByIsEnabled()
    {
        var pc = Make(Guid.NewGuid(), Guid.NewGuid(), enabled: false);
        var paged = new PagedResult<PartnerCountry>(new() { pc }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPartnerCountriesQuery>(q => q.IsEnabled == false),
                            1, 10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/partner-countries?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }


    [Fact(DisplayName = "GET /api/partner-countries uses default paging")]
    public async Task Get_ShouldUseDefaults()
    {
        var list = new[]
        {
                Make(Guid.NewGuid(), Guid.NewGuid()),
                Make(Guid.NewGuid(), Guid.NewGuid()),
                Make(Guid.NewGuid(), Guid.NewGuid())
            };
        var paged = new PagedResult<PartnerCountry>(list.ToList(), 3, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPartnerCountriesQuery>(q =>
                                q.PageNumber == 1 &&
                                q.PageSize == 10 &&
                                q.IsEnabled == true),
                            1, 10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/partner-countries");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
    }


    [Fact(DisplayName = "GET /api/partner-countries with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var pc = Make(partnerId, countryId);
        var paged = new PagedResult<PartnerCountry>(new() { pc }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<GetFiltredPartnerCountriesQuery>(),
                            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var url = $"/api/partner-countries?partnerId={partnerId}&countryId={countryId}&isEnabled=true";
        var res = await _client.GetAsync(url);
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("partnerId").GetGuid().Should().Be(partnerId);
        dto.Items[0].GetProperty("countryId").GetGuid().Should().Be(countryId);
    }


    [Fact(DisplayName = "GET /api/partner-countries returns empty list when no match")]
    public async Task Get_ShouldReturnEmpty_WhenNoMatch()
    {
        var paged = new PagedResult<PartnerCountry>(new(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredPartnerCountriesQuery>(q => q.CountryId == Guid.Empty),
                            1, 10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/partner-countries?countryId=00000000-0000-0000-0000-000000000000");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}