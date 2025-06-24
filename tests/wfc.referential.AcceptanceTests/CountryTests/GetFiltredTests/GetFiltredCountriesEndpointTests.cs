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
using wfc.referential.Application.Countries.Queries.GetFiltredCounties;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.GetFiltredTests;

public class GetFiltredCountriesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryRepository> _repoMock = new();

    public GetFiltredCountriesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<ICountryRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    private static Country CreateCountry(string code, string name, bool enabled = true)
    {
        var country = Country.Create(
            CountryId.Of(Guid.NewGuid()),
            abbreviation: code,
            name,
            code,
            ISO2: code.Length >= 2 ? code[..2] : code,
            ISO3: code.Length >= 3 ? code[..3] : code + "X",
            dialingCode: "+1",
            timeZone: "+0",
            hasSector: false,
            isSmsEnabled: false,
            numberDecimalDigits: 2,
            monetaryZoneId: MonetaryZoneId.Of(Guid.NewGuid()),
            currencyId: CurrencyId.Of(Guid.NewGuid()));

        if (!enabled) country.Disable();
        return country;
    }

    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);


    [Fact(DisplayName = "GET /api/countries returns paged list")]
    public async Task Get_ShouldReturnPagedList()
    {
        var items = new[] { CreateCountry("US", "United States"),
                                CreateCountry("FR", "France") };

        var paged = new PagedResult<Country>(items.ToList(), totalCount: 4, pageNumber: 1, pageSize: 2);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.Is<GetFiltredCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                1,
                2,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Country, object>>[]>()    
        )).ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/countries?pageNumber=1&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalPages.Should().Be(2);
    }

    [Fact(DisplayName = "GET /api/countries?name=France filters by Name")]
    public async Task Get_ShouldFilterByName()
    {
        var country = CreateCountry("FR", "France");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountriesQuery>(q => q.Name == "France"),
                            1, 10, It.IsAny<CancellationToken>(),
                            It.IsAny<Expression<Func<Country, object>>[]>()))
                 .ReturnsAsync(new PagedResult<Country>(new() { country }, 1, 1, 10));

        var res = await _client.GetAsync("/api/countries?name=France");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items[0].GetProperty("name").GetString().Should().Be("France");
    }

    [Fact(DisplayName = "GET /api/countries?code=US filters by Code")]
    public async Task Get_ShouldFilterByCode()
    {
        var country = CreateCountry("US", "United States");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountriesQuery>(q => q.Code == "US"),
                            1, 10, It.IsAny<CancellationToken>(),
                            It.IsAny<Expression<Func<Country, object>>[]>()))
                 .ReturnsAsync(new PagedResult<Country>(new() { country }, 1, 1, 10));

        var res = await _client.GetAsync("/api/countries?code=US");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("code").GetString().Should().Be("US");
    }

    [Fact(DisplayName = "GET /api/countries?isEnabled=false filters disabled")]
    public async Task Get_ShouldFilterByEnabled()
    {
        var disabled = CreateCountry("ZZ", "Disabledland", enabled: false);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountriesQuery>(q => q.IsEnabled == false),
                            1, 10, It.IsAny<CancellationToken>(),
                            It.IsAny<Expression<Func<Country, object>>[]>()))
                 .ReturnsAsync(new PagedResult<Country>(new() { disabled }, 1, 1, 10));

        var res = await _client.GetAsync("/api/countries?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/countries default paging when no params")]
    public async Task Get_ShouldUseDefaults()
    {
        var list = new[] { CreateCountry("A", "A"), CreateCountry("B", "B"), CreateCountry("C", "C") };
        var paged = new PagedResult<Country>(list.ToList(), 3, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountriesQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                            1, 10, It.IsAny<CancellationToken>(),
                            It.IsAny<Expression<Func<Country, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/countries");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "GET /api/countries with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        var zoneId = Guid.NewGuid();

        var country = CreateCountry("AA", "Atlantis");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountriesQuery>(q =>
                                q.Code == "AA" &&
                                q.Name == "Atlantis" &&
                                q.MonetaryZoneId == zoneId &&
                                q.IsEnabled == true),
                            1, 10, It.IsAny<CancellationToken>(), 
                            It.IsAny<Expression<Func<Country, object>>[]>()))
                 .ReturnsAsync(new PagedResult<Country>(new() { country }, 1, 1, 10));

        var res = await _client.GetAsync($"/api/countries?code=AA&name=Atlantis&monetaryZoneId={zoneId}&isEnabled=true");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("AA");
    }

    [Fact(DisplayName = "GET /api/countries returns empty page when no match")]
    public async Task Get_ShouldReturnEmpty_WhenNoMatch()
    {
        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountriesQuery>(q => q.Name == "Neverland"),
                            1, 10, It.IsAny<CancellationToken>(), 
                            It.IsAny<Expression<Func<Country, object>>[]>()))
                 .ReturnsAsync(new PagedResult<Country>(new(), 0, 1, 10));

        var res = await _client.GetAsync("/api/countries?name=Neverland");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}