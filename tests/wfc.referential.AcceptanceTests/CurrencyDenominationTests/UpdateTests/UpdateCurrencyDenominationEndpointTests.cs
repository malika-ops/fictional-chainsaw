using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyDenominationTests.UpdateTests;

public class UpdateCurrencyDenominationAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PUT /api/currencyDenominations/{id} updates all fields of existing currency denomination")]
    public async Task UpdateCurrencyDenomination_Should_UpdateAllFields_WhenCurrencyDenominationExists()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var existingCurrencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")),
            CurrencyDenominationType.Coin,
            100);

        var newCurrencyId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var currency = Currency.Create(CurrencyId.Of(newCurrencyId), "EUR", "يورو", "Euro", "Euro", 978);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrencyDenomination);
        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == newCurrencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);
        _currencyDenominationRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CurrencyDenomination, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        var updateRequest = new UpdateCurrencyDenominationRequest
        {
            CurrencyId = newCurrencyId,
            Type = CurrencyDenominationType.Banknote,
            Value = 500,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify all fields were updated
        existingCurrencyDenomination.CurrencyId.Should().Be(CurrencyId.Of(newCurrencyId));
        existingCurrencyDenomination.Type.Should().Be(CurrencyDenominationType.Banknote);
        existingCurrencyDenomination.Value.Should().Be(500);
        existingCurrencyDenomination.IsEnabled.Should().BeFalse();

        _currencyDenominationRepoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()), Times.Once);
        _currencyRepoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == newCurrencyId), It.IsAny<CancellationToken>()), Times.Once);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/currencyDenominations/{id} validates currency exists before update")]
    public async Task UpdateCurrencyDenomination_Should_ValidateCurrencyExists_BeforeUpdate()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var nonExistentCurrencyId = Guid.NewGuid();
        var existingCurrencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")),
            CurrencyDenominationType.Coin,
            100);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrencyDenomination);
        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == nonExistentCurrencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var updateRequest = new UpdateCurrencyDenominationRequest
        {
            CurrencyId = nonExistentCurrencyId,
            Type = CurrencyDenominationType.Banknote,
            Value = 200,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencyDenominations/{id} validates all required fields are provided")]
    public async Task UpdateCurrencyDenomination_Should_ValidateAllRequiredFields_AreProvided()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var currencyId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1");
        var existingCurrencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(currencyId),
            CurrencyDenominationType.Coin,
            100);

        var currency = Currency.Create(CurrencyId.Of(currencyId), "MAD", "درهم مغربي", "Moroccan Dirham", "Moroccan Dirham", 504);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrencyDenomination);
        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);
        _currencyDenominationRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CurrencyDenomination, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        var updateRequest = new UpdateCurrencyDenominationRequest
        {
            CurrencyId = currencyId,
            Type = CurrencyDenominationType.Banknote,
            Value = 200,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify all fields were properly set
        existingCurrencyDenomination.CurrencyId.Should().Be(CurrencyId.Of(currencyId));
        existingCurrencyDenomination.Type.Should().Be(CurrencyDenominationType.Banknote);
        existingCurrencyDenomination.Value.Should().Be(200);
        existingCurrencyDenomination.IsEnabled.Should().BeFalse();

        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}