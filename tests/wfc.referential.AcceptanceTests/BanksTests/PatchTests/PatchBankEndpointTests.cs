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

namespace wfc.referential.AcceptanceTests.BanksTests.PatchTests;

public class PatchBankEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public PatchBankEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
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

    // Helper to create a test bank
    private static Bank CreateTestBank(Guid id, string code, string name, string abbreviation)
    {
        return Bank.Create(new BankId(id), code, name, abbreviation);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bank = CreateTestBank(id, "OLD-CODE", "Old Name", "ON");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(bank);

        _repoMock.Setup(r => r.GetByCodeAsync("NEW-CODE", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Bank?)null);   // Code is unique

        Bank? updated = null;
        _repoMock.Setup(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Bank, CancellationToken>((b, _) => updated = b)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            BankId = id,
            Code = "NEW-CODE"
            // Name and Abbreviation intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/banks/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("NEW-CODE");
        updated.Name.Should().Be("Old Name");  // Name should not change
        updated.Abbreviation.Should().Be("ON"); // Abbreviation should not change

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} returns 400 when bank doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenBankDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Bank?)null);

        var payload = new
        {
            BankId = id,
            Name = "New Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/banks/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Bank not found");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} returns 400 when new code already exists")]
    public async Task Patch_ShouldReturn400_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestBank(existingId, "DUPE-CODE", "Existing Bank", "EB");
        var target = CreateTestBank(id, "OLD-CODE", "Target Bank", "TB");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("DUPE-CODE", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate code

        var payload = new
        {
            BankId = id,
            Code = "DUPE-CODE"  // This code already exists for another bank
        };

        // Act
        var response = await _client.PatchAsync($"/api/banks/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Bank with code DUPE-CODE already exists.");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} allows changing only the bank's enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyStatus_WhenPatchingStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bank = CreateTestBank(id, "BANK-001", "Test Bank", "TB");
        bank.Update("BANK-001", "Test Bank", "TB"); // Ensure initial status is Enabled

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(bank);

        Bank? updated = null;
        _repoMock.Setup(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Bank, CancellationToken>((b, _) => updated = b)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            BankId = id,
            IsEnabled = false // Changed from true (enabled) to false (disabled)
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/banks/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();
        updated.Code.Should().Be("BANK-001");     // Unchanged
        updated.Name.Should().Be("Test Bank");    // Unchanged
        updated.Abbreviation.Should().Be("TB");   // Unchanged

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                          Times.Once);
    }
}