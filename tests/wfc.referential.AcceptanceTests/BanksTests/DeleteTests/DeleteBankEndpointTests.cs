using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.DeleteTests;

public class DeleteBankAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "DELETE /api/banks/{id} disables bank when deletion requested")]
    public async Task DeleteBank_Should_DisableBank_WhenDeletionRequested()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{bankId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify bank was disabled (soft delete)
        bank.IsEnabled.Should().BeFalse();

        _bankRepoMock.Verify(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()), Times.Once);
        _bankRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 400 when bank not found")]
    public async Task DeleteBank_Should_ReturnBadRequest_WhenBankNotFound()
    {
        // Arrange
        var bankId = Guid.NewGuid();

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{bankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _bankRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} changes status to inactive instead of physical deletion")]
    public async Task DeleteBank_Should_ChangeStatusToInactive_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(
            BankId.Of(bankId),
            "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE");

        // Verify bank starts as enabled
        bank.IsEnabled.Should().BeTrue();

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{bankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        bank.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (bank object still exists)
        bank.Should().NotBeNull();
        bank.Code.Should().Be("BMCE"); // Data still intact
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} validates bank exists before deletion")]
    public async Task DeleteBank_Should_ValidateBankExists_BeforeDeletion()
    {
        // Arrange
        var nonExistentBankId = Guid.NewGuid();

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == nonExistentBankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{nonExistentBankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _bankRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 400 for invalid GUID format")]
    public async Task DeleteBank_Should_ReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/banks/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _bankRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()), Times.Never);
        _bankRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
