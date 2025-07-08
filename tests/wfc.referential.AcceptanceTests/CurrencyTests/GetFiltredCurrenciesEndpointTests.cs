using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.SearchTests;

public class GetFiltredCurrenciesAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "GET /api/currencies returns all currencies using search criteria")]
    public async Task GetFiltredCurrencies_Should_ReturnAllCurrencies_UsingSearchCriteria()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "GBP", "جنيه إسترليني", "British Pound", "British Pound", 826)
        };

        var pagedResult = new PagedResult<Currency>(currencies, currencies.Count, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);

        // Verify all currency data is returned
        result.Items.Should().Contain(c => c.Code == "USD" && c.CodeIso == 840);
        result.Items.Should().Contain(c => c.Code == "EUR" && c.CodeIso == 978);
        result.Items.Should().Contain(c => c.Code == "GBP" && c.CodeIso == 826);
    }

    [Fact(DisplayName = "GET /api/currencies supports filtering by CurrencyCode")]
    public async Task GetFiltredCurrencies_Should_FilterByCurrencyCode_WhenCodeProvided()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840)
        };

        var pagedResult = new PagedResult<Currency>(filteredCurrencies, 1, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?Code=USD&PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("USD");
    }

    [Fact(DisplayName = "GET /api/currencies supports filtering by CurrencyName")]
    public async Task GetFiltredCurrencies_Should_FilterByCurrencyName_WhenNameProvided()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "AUD", "دولار أسترالي", "Australian Dollar", "Australian Dollar", 36)
        };

        var pagedResult = new PagedResult<Currency>(filteredCurrencies, 2, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?Name=Dollar&PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(c => c.Name.Should().Contain("Dollar"));
    }

    [Fact(DisplayName = "GET /api/currencies supports filtering by IsEnabled status")]
    public async Task GetFiltredCurrencies_Should_FilterByIsEnabled_WhenStatusProvided()
    {
        // Arrange
        var activeCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978)
        };

        var pagedResult = new PagedResult<Currency>(activeCurrencies, 2, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?IsEnabled=true&PageNumber=1&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(c => c.IsEnabled.Should().BeTrue());
    }

    [Fact(DisplayName = "GET /api/currencies returns paginated results")]
    public async Task GetFiltredCurrencies_Should_ReturnPaginatedResults_WhenPaginationParametersProvided()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840),
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "EUR", "يورو", "Euro", "Euro", 978)
        };

        var pagedResult = new PagedResult<Currency>(currencies, 25, 2, 10); // Page 2 of 3 pages (25 total items)

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?PageNumber=2&PageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GET /api/currencies returns empty result when no matches found")]
    public async Task GetFiltredCurrencies_Should_ReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<Currency>(new List<Currency>(), 0, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/currencies?Code=NONEXISTENT");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/currencies supports multiple search criteria simultaneously")]
    public async Task GetFiltredCurrencies_Should_SupportMultipleCriteria_WhenMultipleFiltersProvided()
    {
        // Arrange
        var filteredCurrencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840)
        };

        var pagedResult = new PagedResult<Currency>(filteredCurrencies, 1, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act - Search with multiple criteria
        var response = await _client.GetAsync("/api/currencies?Code=USD&CodeIso=840&IsEnabled=true");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var currency = result.Items.First();
        currency.Code.Should().Be("USD");
        currency.CodeIso.Should().Be(840);
        currency.IsEnabled.Should().BeTrue();
    }

    [Theory(DisplayName = "GET /api/currencies supports various search criteria")]
    [InlineData("Code", "USD")]
    [InlineData("Name", "Dollar")]
    [InlineData("CodeAR", "دولار")]
    [InlineData("CodeEN", "US Dollar")]
    public async Task GetFiltredCurrencies_Should_SupportVariousSearchCriteria(string filterType, string filterValue)
    {
        // Arrange
        var currencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840)
        };

        var pagedResult = new PagedResult<Currency>(currencies, 1, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/currencies?{filterType}={filterValue}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        _currencyRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies returns all currency data in response")]
    public async Task GetFiltredCurrencies_Should_ReturnAllCurrencyData_InResponse()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            Currency.Create(CurrencyId.Of(Guid.NewGuid()), "USD", "دولار أمريكي", "US Dollar", "United States Dollar", 840)
        };

        var pagedResult = new PagedResult<Currency>(currencies, 1, 1, 10);

        _currencyRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/currencies");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetCurrenciesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var currency = result.Items.First();
        currency.CurrencyId.Should().NotBeEmpty();
        currency.Code.Should().Be("USD");
        currency.CodeAR.Should().Be("دولار أمريكي");
        currency.CodeEN.Should().Be("US Dollar");
        currency.Name.Should().Be("United States Dollar");
        currency.CodeIso.Should().Be(840);
        currency.IsEnabled.Should().BeTrue();
    }
}