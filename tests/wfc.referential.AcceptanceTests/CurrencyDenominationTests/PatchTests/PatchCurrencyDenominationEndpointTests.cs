using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyDenominationTests.PatchTests;

public class PatchCurrencyDenominationAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PATCH /api/currencyDenominations/{id} modifies only provided fields")]
    public async Task PatchCurrencyDenomination_Should_ModifyOnlyProvidedFields_WhenPartialDataSent()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var originalCurrencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")),
            CurrencyDenominationType.Coin,
            100);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalCurrencyDenomination);
        _currencyDenominationRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CurrencyDenomination, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        var patchRequest = new PatchCurrencyDenominationRequest
        {
            Value = 200, // Only updating value
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only the value was changed, other fields remain unchanged
        originalCurrencyDenomination.Value.Should().Be(200);
        originalCurrencyDenomination.Type.Should().Be(CurrencyDenominationType.Coin); // Unchanged
        originalCurrencyDenomination.CurrencyId.Should().Be(CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1"))); // Unchanged

        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencyDenominations/{id} allows same values in unchanged fields")]
    public async Task PatchCurrencyDenomination_Should_AllowSameValues_WhenNotChangingFields()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var currencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")),
            CurrencyDenominationType.Coin,
            100);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencyDenomination);
        _currencyDenominationRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CurrencyDenomination, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        var patchRequest = new PatchCurrencyDenominationRequest
        {
            Type = CurrencyDenominationType.Coin, // Same type as current (should be allowed)
            Value = 200 // Different value
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencyDenominations/{id} returns 404 when currency denomination not found")]
    public async Task PatchCurrencyDenomination_Should_ReturnNotFound_WhenCurrencyDenominationNotFound()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        var patchRequest = new PatchCurrencyDenominationRequest
        {
            Value = 300
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/currencyDenominations/{id} validates currency exists when updating currency reference")]
    public async Task PatchCurrencyDenomination_Should_ValidateCurrencyExists_WhenUpdatingCurrencyReference()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var newCurrencyId = Guid.NewGuid();
        var currencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")),
            CurrencyDenominationType.Coin,
            100);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencyDenomination);

        // Mock that the new currency doesn't exist
        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == newCurrencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyDenominationRequest
        {
            CurrencyId = newCurrencyId
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencyDenominations/{currencyDenominationId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}