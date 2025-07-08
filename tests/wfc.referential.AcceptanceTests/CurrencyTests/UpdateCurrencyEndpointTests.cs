using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.UpdateTests;

public class UpdateCurrencyAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PUT /api/currencies/{id} modifies currency data")]
    public async Task UpdateCurrency_Should_ModifyAllCurrencyFields_WhenValidDataProvided()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var updateRequest = new UpdateCurrencyRequest
        {
            Code = "USD_UPDATED",
            CodeAR = "دولار أمريكي محدث",
            CodeEN = "Updated US Dollar",
            Name = "Updated United States Dollar",
            CodeIso = 841,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _currencyRepoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Once);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} validates Code uniqueness before update")]
    public async Task UpdateCurrency_Should_ValidateCodeUniqueness_BeforeUpdate()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var targetCurrency = Currency.Create(CurrencyId.Of(currencyId), "USD", "دولار", "Dollar", "Dollar", 840);
        var conflictingCurrency = Currency.Create(CurrencyId.Of(existingCurrencyId), "EUR", "يورو", "Euro", "Euro", 978);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetCurrency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingCurrency);

        var updateRequest = new UpdateCurrencyRequest
        {
            Code = "EUR", // Code already exists
            Name = "Updated Currency",
            CodeAR = "عملة محدثة",
            CodeEN = "Updated Currency",
            CodeIso = 999,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} validates CodeIso uniqueness before update")]
    public async Task UpdateCurrency_Should_ValidateCodeIsoUniqueness_BeforeUpdate()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var targetCurrency = Currency.Create(CurrencyId.Of(currencyId), "USD", "دولار", "Dollar", "Dollar", 840);
        var conflictingCurrency = Currency.Create(CurrencyId.Of(existingCurrencyId), "EUR", "يورو", "Euro", "Euro", 978);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetCurrency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingCurrency);

        var updateRequest = new UpdateCurrencyRequest
        {
            Code = "TEST",
            Name = "Test Currency",
            CodeAR = "عملة اختبار",
            CodeEN = "Test Currency",
            CodeIso = 978, // ISO code already exists
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} returns 400 when currency not found")]
    public async Task UpdateCurrency_Should_ReturnBadRequest_WhenCurrencyNotFound()
    {
        // Arrange
        var currencyId = Guid.NewGuid();

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var updateRequest = new UpdateCurrencyRequest
        {
            Code = "TEST",
            Name = "Test Currency",
            CodeAR = "عملة اختبار",
            CodeEN = "Test Currency",
            CodeIso = 123,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _currencyRepoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} verifies value after update")]
    public async Task UpdateCurrency_Should_VerifyUpdatedValues_AfterSuccessfulUpdate()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD", "دولار", "Dollar", "Dollar", 840);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);
        _currencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Currency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var updateRequest = new UpdateCurrencyRequest
        {
            Code = "UPDATED",
            Name = "Updated Name",
            CodeAR = "اسم محدث",
            CodeEN = "Updated Name",
            CodeIso = 999,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify currency was updated with new values
        currency.Code.Should().Be("UPDATED");
        currency.Name.Should().Be("Updated Name");
        currency.CodeAR.Should().Be("اسم محدث");
        currency.CodeEN.Should().Be("Updated Name");
        currency.CodeIso.Should().Be(999);
        currency.IsEnabled.Should().BeFalse();
    }
}