using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.UpdateTests;

public class UpdateBankAcceptanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public UpdateBankAcceptanceTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();

                _repoMock.Setup(r => r.Update(It.IsAny<Bank>()));
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
            });
        });
        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "PUT /api/banks/{id} modifies bank data")]
    public async Task UpdateBank_Should_ModifyAllBankFields_WhenValidDataProvided()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var existingBank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBank);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var updateRequest = new UpdateBankRequest
        {
            BankId = bankId,
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

        _repoMock.Verify(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} validates Code uniqueness before update")]
    public async Task UpdateBank_Should_ValidateCodeUniqueness_BeforeUpdate()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var existingBankId = Guid.NewGuid();

        var targetBank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");
        var conflictingBank = Bank.Create(BankId.Of(existingBankId), "BMCE", "Banque Marocaine du Commerce Extérieur", "BMCE");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetBank);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingBank);

        var updateRequest = new UpdateBankRequest
        {
            BankId = bankId,
            Code = "BMCE", // Code already exists
            Name = "Updated Bank",
            Abbreviation = "UB",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{bankId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} returns 400 when bank not found")]
    public async Task UpdateBank_Should_ReturnBadRequest_WhenBankNotFound()
    {
        // Arrange
        var bankId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var updateRequest = new UpdateBankRequest
        {
            BankId = bankId,
            Code = "TEST",
            Name = "Test Bank",
            Abbreviation = "TB",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{bankId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} verifies value after update")]
    public async Task UpdateBank_Should_VerifyUpdatedValues_AfterSuccessfulUpdate()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var updateRequest = new UpdateBankRequest
        {
            BankId = bankId,
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