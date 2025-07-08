using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.CreateTests;

public class CreateCurrencyAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "POST /api/currencies creates currency with all required fields")]
    public async Task CreateCurrency_Should_CreateNewCurrency_WhenAllRequiredFieldsProvided()
    {
        // Arrange
        var createRequest = new CreateCurrencyRequest
        {
            Code = "USD",
            CodeAR = "دولار أمريكي",
            CodeEN = "US Dollar",
            Name = "United States Dollar",
            CodeIso = 840
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", createRequest);
        var currencyId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        currencyId.Should().NotBeEmpty();

        _currencyRepoMock.Verify(r => r.AddAsync(It.Is<Currency>(c =>
            c.Code == "USD" &&
            c.CodeAR == "دولار أمريكي" &&
            c.CodeEN == "US Dollar" &&
            c.Name == "United States Dollar" &&
            c.CodeIso == 840 &&
            c.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);

        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when Code is empty")]
    public async Task CreateCurrency_Should_ReturnValidationError_WhenCurrencyCodeIsEmpty()
    {
        // Arrange
        var invalidRequest = new CreateCurrencyRequest
        {
            Code = "",
            Name = "Test Currency",
            CodeAR = "عملة اختبار",
            CodeEN = "Test Currency",
            CodeIso = 123
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _currencyRepoMock.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when duplicate Code is provided")]
    public async Task CreateCurrency_Should_ReturnConflictError_WhenCurrencyCodeAlreadyExists()
    {
        // Arrange
        var existingCurrency = Currency.Create(
            CurrencyId.Of(Guid.NewGuid()),
            "EUR", "يورو", "Euro", "Euro", 978);

        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var duplicateRequest = new CreateCurrencyRequest
        {
            Code = "EUR",
            Name = "European Currency",
            CodeAR = "يورو",
            CodeEN = "Euro",
            CodeIso = 978
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _currencyRepoMock.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when duplicate CodeIso is provided")]
    public async Task CreateCurrency_Should_ReturnConflictError_WhenCodeIsoAlreadyExists()
    {
        // Arrange
        var existingCurrency = Currency.Create(
            CurrencyId.Of(Guid.NewGuid()),
            "EUR", "يورو", "Euro", "Euro", 978);

        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var duplicateRequest = new CreateCurrencyRequest
        {
            Code = "USD",
            Name = "US Dollar",
            CodeAR = "دولار أمريكي",
            CodeEN = "US Dollar",
            CodeIso = 978 // Same ISO code as existing
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _currencyRepoMock.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/currencies auto-generates currency ID")]
    public async Task CreateCurrency_Should_AutoGenerateCurrencyId_WhenCurrencyIsCreated()
    {
        // Arrange
        var createRequest = new CreateCurrencyRequest
        {
            Code = "JPY",
            Name = "Japanese Yen",
            CodeAR = "ين ياباني",
            CodeEN = "Japanese Yen",
            CodeIso = 392
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", createRequest);
        var currencyId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        currencyId.Should().NotBeEmpty();

        _currencyRepoMock.Verify(r => r.AddAsync(It.Is<Currency>(c =>
            c.Id != null && c.Id.Value != Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/currencies sets IsEnabled to true by default")]
    public async Task CreateCurrency_Should_SetIsEnabledToTrue_ByDefault()
    {
        // Arrange
        var createRequest = new CreateCurrencyRequest
        {
            Code = "GBP",
            Name = "British Pound",
            CodeAR = "جنيه إسترليني",
            CodeEN = "British Pound",
            CodeIso = 826
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _currencyRepoMock.Verify(r => r.AddAsync(It.Is<Currency>(c =>
            c.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when CodeIso is out of range")]
    public async Task CreateCurrency_Should_ReturnValidationError_WhenCodeIsoIsOutOfRange()
    {
        // Arrange
        var invalidRequest = new CreateCurrencyRequest
        {
            Code = "TEST",
            Name = "Test Currency",
            CodeAR = "عملة اختبار",
            CodeEN = "Test Currency",
            CodeIso = 1234 // More than 3 digits
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _currencyRepoMock.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "POST /api/currencies validates all required fields")]
    [InlineData("", "Name", "CodeAR", "CodeEN", 123)]
    [InlineData("CODE", "", "CodeAR", "CodeEN", 123)]
    [InlineData("CODE", "Name", "", "CodeEN", 123)]
    [InlineData("CODE", "Name", "CodeAR", "", 123)]
    public async Task CreateCurrency_Should_ReturnValidationError_WhenRequiredFieldsAreMissing(
        string code, string name, string codeAR, string codeEN, int codeIso)
    {
        // Arrange
        var invalidRequest = new CreateCurrencyRequest
        {
            Code = code,
            Name = name,
            CodeAR = codeAR,
            CodeEN = codeEN,
            CodeIso = codeIso
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _currencyRepoMock.Verify(r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
