using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.UpdateTests;

public class UpdateBankAcceptanceTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PUT /api/banks/{id} modifies bank data")]
    public async Task UpdateBank_Should_ModifyAllBankFields_WhenValidDataProvided()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var existingBank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBank);
        _bankRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var updateRequest = new UpdateBankRequest
        {
            Code = "AWB_UPDATED",
            Name = "Updated Attijariwafa Bank",
            Abbreviation = "AWB-U",
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{bankId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _bankRepoMock.Verify(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()), Times.Once);
        _bankRepoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Once);
        _bankRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} validates Code uniqueness before update")]
    public async Task UpdateBank_Should_ValidateCodeUniqueness_BeforeUpdate()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var existingBankId = Guid.NewGuid();

        var targetBank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");
        var conflictingBank = Bank.Create(BankId.Of(existingBankId), "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE");

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBank);
        _bankRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingBank);

        var updateRequest = new UpdateBankRequest
        {
            Code = "BMCE", // Code already exists
            Name = "Updated Bank",
            Abbreviation = "UB",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{bankId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _bankRepoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} returns 400 when bank not found")]
    public async Task UpdateBank_Should_ReturnBadRequest_WhenBankNotFound()
    {
        // Arrange
        var bankId = Guid.NewGuid();

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var updateRequest = new UpdateBankRequest
        {
            Code = "TEST",
            Name = "Test Bank",
            Abbreviation = "TB",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{bankId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _bankRepoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} verifies value after update")]
    public async Task UpdateBank_Should_VerifyUpdatedValues_AfterSuccessfulUpdate()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _bankRepoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);
        _bankRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var updateRequest = new UpdateBankRequest
        {
            Code = "UPDATED",
            Name = "Updated Name",
            Abbreviation = "UN",
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{bankId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify bank was updated with new values
        bank.Code.Should().Be("UPDATED");
        bank.Name.Should().Be("Updated Name");
        bank.Abbreviation.Should().Be("UN");
        bank.IsEnabled.Should().BeFalse();
    }
}