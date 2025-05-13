using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
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

public class DeleteBankEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public DeleteBankEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy banks quickly
    private static Bank CreateTestBank(Guid id, string code, string name, string abbreviation)
    {
        return Bank.Create(new BankId(id), code, name, abbreviation);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 200 when bank exists and has no linked accounts")]
    public async Task Delete_ShouldReturn200_WhenBankExistsAndHasNoLinkedAccounts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bank = CreateTestBank(id, "TEST-001", "Test Bank", "TB");

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);

        _repoMock
            .Setup(r => r.HasLinkedAccountsAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Capture the entity passed to Update
        Bank? updatedBank = null;
        _repoMock
            .Setup(r => r.UpdateBankAsync(It.IsAny<Bank>(), It.IsAny<CancellationToken>()))
            .Callback<Bank, CancellationToken>((b, _) => updatedBank = b)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedBank!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 400 when bank is not found")]
    public async Task Delete_ShouldReturn400_WhenBankNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Bank not found");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/banks/{id} returns 400 when bank has linked accounts")]
    public async Task Delete_ShouldReturn400_WhenBankHasLinkedAccounts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bank = CreateTestBank(id, "TEST-002", "Test Bank", "TB");

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bank);

        _repoMock
            .Setup(r => r.HasLinkedAccountsAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);  // Has linked accounts

        // Act
        var response = await _client.DeleteAsync($"/api/banks/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Cannot delete bank with ID {id} because it is linked to one or more accounts.");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Never);
    }
}