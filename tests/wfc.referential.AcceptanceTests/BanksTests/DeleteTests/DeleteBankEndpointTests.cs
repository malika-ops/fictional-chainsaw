using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.BanksTests.DeleteTests;

public class DeleteBankAcceptanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public DeleteBankAcceptanceTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();

                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
            });
        });
        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} disables bank when deletion requested")]
    public async Task DeleteBank_Should_DisableBank_WhenDeletionRequested()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{bankId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify bank was disabled (soft delete)
        bank.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 400 when bank not found")]
    public async Task DeleteBank_Should_ReturnBadRequest_WhenBankNotFound()
    {
        // Arrange
        var bankId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{bankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
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

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == nonExistentBankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{nonExistentBankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 400 for invalid GUID format")]
    public async Task DeleteBank_Should_ReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/banks/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no repository operations were attempted
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
