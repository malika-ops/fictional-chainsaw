using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Currencies.Queries;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.GetTests;

public class GetAllCurrenciesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICurrencyRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    public GetAllCurrenciesEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ICurrencyRepository>();
                services.RemoveAll<ICacheService>();

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "GET /api/currencies returns 200 and paginated result")]
    public async Task Get_ShouldReturn200_AndPaginatedResult_WhenNoFiltersAreApplied()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "GBP", "جنيه إسترليني", "British Pound", "British Pound", 826),
        };

        var totalCount = currencies.Count;
        var pageSize = 10;
        var pageNumber = 1;

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.IsAny<GetAllCurrenciesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.IsAny<GetAllCurrenciesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(currencies.Count);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);

        // Verify currency properties, including CodeIso
        foreach (var item in result.Items)
        {
            var originalCurrency = currencies.First(c => c.Id.Value == item.CurrencyId);
            item.CodeIso.Should().Be(originalCurrency.CodeIso);
        }

        // Verify repository interactions
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize &&
                q.Code == null &&
                q.Name == null &&
                q.CodeIso == null &&
                q.IsEnabled == true),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns 200 and filtered results when filters are applied")]
    public async Task Get_ShouldReturn200_AndFilteredResults_WhenFiltersAreApplied()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "AUD", "دولار أسترالي", "Australian Dollar", "Australian Dollar", 36)
        };

        var totalCount = filteredCurrencies.Count;
        var pageSize = 10;
        var pageNumber = 1;
        var codeFilter = "EU";

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.Code == codeFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredCurrencies);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.Code == codeFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}&Code={codeFilter}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(filteredCurrencies.Count);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);

        // Verify currency properties, including CodeIso
        foreach (var item in result.Items)
        {
            var originalCurrency = filteredCurrencies.First(c => c.Id.Value == item.CurrencyId);
            item.CodeIso.Should().Be(originalCurrency.CodeIso);
        }

        // Verify repository interactions
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize &&
                q.Code == codeFilter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns 200 and filtered results when filtering by codeiso")]
    public async Task Get_ShouldReturn200_AndFilteredResults_WhenFilteringByCodeIso()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978)
        };

        var totalCount = filteredCurrencies.Count;
        var pageSize = 10;
        var pageNumber = 1;
        var codeIsoFilter = 978;

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.CodeIso == codeIsoFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredCurrencies);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.CodeIso == codeIsoFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}&CodeIso={codeIsoFilter}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(filteredCurrencies.Count);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);

        // Verify currency properties
        result.Items.First().CodeIso.Should().Be(codeIsoFilter);
        result.Items.First().Code.Should().Be("EUR");

        // Verify repository interactions
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize &&
                q.CodeIso == codeIsoFilter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns 200 and empty results when no data matches criteria")]
    public async Task Get_ShouldReturn200_AndEmptyResults_WhenNoDataMatchesCriteria()
    {
        // Arrange
        var emptyList = new List<Currency>();
        var totalCount = 0;
        var pageSize = 10;
        var pageNumber = 1;
        var codeFilter = "XYZ"; // Non-existent code

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.Code == codeFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.Code == codeFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}&Code={codeFilter}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize &&
                q.Code == codeFilter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns filtered results when searching by name")]
    public async Task Get_ShouldReturnFilteredResults_WhenSearchingByName()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "AUD", "دولار أسترالي", "Australian Dollar", "Australian Dollar", 36)
        };

        var totalCount = filteredCurrencies.Count;
        var pageSize = 10;
        var pageNumber = 1;
        var nameFilter = "Dollar";

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.Name == nameFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredCurrencies);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.Name == nameFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}&Name={nameFilter}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(filteredCurrencies.Count);
        result.TotalCount.Should().Be(totalCount);

        // Verify that currency properties including CodeIso are correct
        foreach (var item in result.Items)
        {
            var originalCurrency = filteredCurrencies.First(c => c.Id.Value == item.CurrencyId);
            item.Code.Should().Be(originalCurrency.Code);
            item.CodeIso.Should().Be(originalCurrency.CodeIso);
        }

        // Verify repository interactions
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.Name == nameFilter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns filtered results when filtering by IsEnabled")]
    public async Task Get_ShouldReturnFilteredResults_WhenFilteringByIsEnabled()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "AUD", "دولار أسترالي", "Australian Dollar", "Australian Dollar", 36)
        };

        // Disable the currencies for the test
        foreach (var currency in filteredCurrencies)
        {
            currency.Disable();
        }

        var totalCount = filteredCurrencies.Count;
        var pageSize = 10;
        var pageNumber = 1;
        var isEnabledFilter = false;

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.IsEnabled == isEnabledFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredCurrencies);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.Is<GetAllCurrenciesQuery>(q => q.IsEnabled == isEnabledFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}&IsEnabled={isEnabledFilter}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(filteredCurrencies.Count);
        result.TotalCount.Should().Be(totalCount);

        // Verify currency properties, including CodeIso
        foreach (var item in result.Items)
        {
            var originalCurrency = filteredCurrencies.First(c => c.Id.Value == item.CurrencyId);
            item.CodeIso.Should().Be(originalCurrency.CodeIso);
            item.IsEnabled.Should().Be(isEnabledFilter);
        }

        // Verify repository interactions
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.IsEnabled == isEnabledFilter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns cached results when available")]
    public async Task Get_ShouldReturnCachedResults_WhenAvailable()
    {
        // Arrange
        var cachedResult = new PagedResult<CurrencyResponse>(
            new List<CurrencyResponse>
            {
                new CurrencyResponse(
                    Guid.NewGuid(),
                    "USD",
                    "دولار أمريكي",
                    "US Dollar",
                    "US Dollar",
                    840,
                    true,
                    0)
            },
            1, 1, 10);

        // Configure mock to accept any cache key
        _cacheMock
            .Setup(c => c.GetAsync<PagedResult<CurrencyResponse>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CodeIso.Should().Be(840);

        // Verify cache interactions
        _cacheMock.Verify(c => c.GetAsync<PagedResult<CurrencyResponse>>(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Repository should not be called when cache hit
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.IsAny<GetAllCurrenciesQuery>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "GET /api/currencies with combined filters returns correct results")]
    public async Task Get_ShouldReturnFilteredResults_WhenMultipleFiltersAreApplied()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840)
        };

        var totalCount = filteredCurrencies.Count;
        var pageSize = 10;
        var pageNumber = 1;
        var codeFilter = "USD";
        var codeIsoFilter = 840;

        _repoMock
            .Setup(r => r.GetCurrenciesByCriteriaAsync(
                It.Is<GetAllCurrenciesQuery>(q =>
                    q.Code == codeFilter &&
                    q.CodeIso == codeIsoFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(filteredCurrencies);

        _repoMock
            .Setup(r => r.GetCountTotalAsync(
                It.Is<GetAllCurrenciesQuery>(q =>
                    q.Code == codeFilter &&
                    q.CodeIso == codeIsoFilter),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        // Act
        var response = await _client.GetAsync($"/api/currencies?PageNumber={pageNumber}&PageSize={pageSize}&Code={codeFilter}&CodeIso={codeIsoFilter}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(filteredCurrencies.Count);
        result.TotalCount.Should().Be(totalCount);

        // Verify that the first (and only) currency has the expected properties
        var item = result.Items.First();
        item.Code.Should().Be(codeFilter);
        item.CodeIso.Should().Be(codeIsoFilter);

        // Verify repository interactions with combined filters
        _repoMock.Verify(r => r.GetCurrenciesByCriteriaAsync(
            It.Is<GetAllCurrenciesQuery>(q =>
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize &&
                q.Code == codeFilter &&
                q.CodeIso == codeIsoFilter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}