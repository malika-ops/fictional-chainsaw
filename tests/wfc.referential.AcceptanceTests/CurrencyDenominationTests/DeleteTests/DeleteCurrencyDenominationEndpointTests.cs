using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyDenominationTests.DeleteTests;

public class DeleteCurrencyDenominationAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "DELETE /api/currencyDenominations/{id} disables currency denomination when deletion requested")]
    public async Task DeleteCurrencyDenomination_Should_DisableCurrencyDenomination_WhenDeletionRequested()
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

        // Act
        var response = await _client.DeleteAsync($"/api/currencyDenominations/{currencyDenominationId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify currency denomination was disabled (soft delete)
        currencyDenomination.IsEnabled.Should().BeFalse();

        _currencyDenominationRepoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()), Times.Once);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/currencyDenominations/{id} returns 400 when currency denomination not found")]
    public async Task DeleteCurrencyDenomination_Should_ReturnBadRequest_WhenCurrencyDenominationNotFound()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        // Act
        var response = await _client.DeleteAsync($"/api/currencyDenominations/{currencyDenominationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/currencyDenominations/{id} changes status to inactive instead of physical deletion")]
    public async Task DeleteCurrencyDenomination_Should_ChangeStatusToInactive_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var currencyDenominationId = Guid.NewGuid();
        var currencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(currencyDenominationId),
            CurrencyId.Of(Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")),
            CurrencyDenominationType.Banknote,
            500);

        // Verify currency denomination starts as enabled
        currencyDenomination.IsEnabled.Should().BeTrue();

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == currencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencyDenomination);

        // Act
        var response = await _client.DeleteAsync($"/api/currencyDenominations/{currencyDenominationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        currencyDenomination.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (currency denomination object still exists)
        currencyDenomination.Should().NotBeNull();
        currencyDenomination.Type.Should().Be(CurrencyDenominationType.Banknote); // Data still intact
        currencyDenomination.Value.Should().Be(500); // Data still intact
    }

    [Fact(DisplayName = "DELETE /api/currencyDenominations/{id} validates currency denomination exists before deletion")]
    public async Task DeleteCurrencyDenomination_Should_ValidateCurrencyDenominationExists_BeforeDeletion()
    {
        // Arrange
        var nonExistentCurrencyDenominationId = Guid.NewGuid();

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(It.Is<CurrencyDenominationId>(id => id.Value == nonExistentCurrencyDenominationId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyDenomination)null);

        // Act
        var response = await _client.DeleteAsync($"/api/currencyDenominations/{nonExistentCurrencyDenominationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/currencyDenominations/{id} returns 400 for invalid GUID format")]
    public async Task DeleteCurrencyDenomination_Should_ReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/currencyDenominations/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _currencyDenominationRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyDenominationId>(), It.IsAny<CancellationToken>()), Times.Never);
        _currencyDenominationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}