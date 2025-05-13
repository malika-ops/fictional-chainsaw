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

namespace wfc.referential.AcceptanceTests.BanksTests.UpdateTests;

public class UpdateBankEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public UpdateBankEndpointTests(WebApplicationFactory<Program> factory)
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

    [Fact(DisplayName = "PUT /api/banks/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldBank = CreateTestBank(id, "OLD-BANK", "Old Bank", "OB");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldBank);

        _repoMock.Setup(r => r.GetByCodeAsync("NEW-BANK", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Bank?)null);   // Code is unique

        Bank? updated = null;
        _repoMock.Setup(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Bank, CancellationToken>((b, _) => updated = b)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            BankId = id,
            Code = "NEW-BANK",
            Name = "New Bank Name",
            Abbreviation = "NB",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("NEW-BANK");
        updated.Name.Should().Be("New Bank Name");
        updated.Abbreviation.Should().Be("NB");
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            BankId = id,
            Code = "BANK-001",
            // Name omitted
            Abbreviation = "B1",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
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
            Code = "DUPE-CODE",        // Duplicate
            Name = "Updated Bank",
            Abbreviation = "UB",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Bank with code DUPE-CODE already exists.");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} returns 404 when bank doesn't exist")]
    public async Task Put_ShouldReturn404_WhenBankDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Bank?)null);

        var payload = new
        {
            BankId = id,
            Code = "NEW-CODE",
            Name = "New Name",
            Abbreviation = "NN",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Bank with ID {id} not found");

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/banks/{id} allows changing the bank's enabled status")]
    public async Task Put_ShouldAllowChangingStatus_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldBank = CreateTestBank(id, "BANK-001", "Test Bank", "TB");
        oldBank.Update("BANK-001", "Test Bank", "TB"); // Ensure initial status is Enabled

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(bid => bid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldBank);

        _repoMock.Setup(r => r.GetByCodeAsync("BANK-001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldBank);   // Same code is ok for same bank

        Bank? updated = null;
        _repoMock.Setup(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Bank, CancellationToken>((b, _) => updated = b)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            BankId = id,
            Code = "BANK-001",
            Name = "Test Bank",
            Abbreviation = "TB",
            IsEnabled = false // Changed from true to false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/banks/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateBankAsync(It.IsAny<Bank>(),
                                               It.IsAny<CancellationToken>()),
                          Times.Once);
    }
}