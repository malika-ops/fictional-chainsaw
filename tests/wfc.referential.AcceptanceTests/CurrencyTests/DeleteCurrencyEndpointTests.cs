using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.DeleteTests;

public class DeleteCurrencyAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "DELETE /api/currencies/{id} disables currency when deletion requested")]
    public async Task DeleteCurrency_Should_DisableCurrency_WhenDeletionRequested()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD", "دولار أمريكي", "US Dollar", "US Dollar", 840);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        // Act
        var response = await _client.DeleteAsync($"/api/currencies/{currencyId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify currency was disabled (soft delete)
        currency.IsEnabled.Should().BeFalse();

        _currencyRepoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} returns 400 when currency not found")]
    public async Task DeleteCurrency_Should_ReturnBadRequest_WhenCurrencyNotFound()
    {
        // Arrange
        var currencyId = Guid.NewGuid();

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        // Act
        var response = await _client.DeleteAsync($"/api/currencies/{currencyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} changes status to inactive instead of physical deletion")]
    public async Task DeleteCurrency_Should_ChangeStatusToInactive_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "EUR", "يورو", "Euro", "Euro", 978);

        // Verify currency starts as enabled
        currency.IsEnabled.Should().BeTrue();

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        // Act
        var response = await _client.DeleteAsync($"/api/currencies/{currencyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        currency.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (currency object still exists)
        currency.Should().NotBeNull();
        currency.Code.Should().Be("EUR"); // Data still intact
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} validates currency exists before deletion")]
    public async Task DeleteCurrency_Should_ValidateCurrencyExists_BeforeDeletion()
    {
        // Arrange
        var nonExistentCurrencyId = Guid.NewGuid();

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == nonExistentCurrencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        // Act
        var response = await _client.DeleteAsync($"/api/currencies/{nonExistentCurrencyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} returns 400 for invalid GUID format")]
    public async Task DeleteCurrency_Should_ReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/currencies/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _currencyRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()), Times.Never);
        _currencyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}