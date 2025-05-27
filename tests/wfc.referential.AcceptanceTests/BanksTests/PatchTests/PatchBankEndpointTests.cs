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

namespace wfc.referential.AcceptanceTests.BanksTests.PatchTests;

public class PatchBankAcceptanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IBankRepository> _repoMock = new();

    public PatchBankAcceptanceTests(WebApplicationFactory<Program> factory)
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

    [Fact(DisplayName = "PATCH /api/banks/{id} modifies only provided fields")]
    public async Task PatchBank_Should_ModifyOnlyProvidedFields_WhenPartialDataSent()
    {
        // Arrange
        var bankId = Guid.NewGuid();
        var originalBank = Bank.Create(
            BankId.Of(bankId),
            "AWB", "Attijariwafa Bank", "AWB");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalBank);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Bank, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var patchRequest = new PatchBankRequest
        {
            BankId = bankId,
            Name = "Updated Bank Name", // Only updating name
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/banks/{bankId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only the name was changed, other fields remain unchanged
        originalBank.Name.Should().Be("Updated Bank Name");
        originalBank.Code.Should().Be("AWB"); // Unchanged
        originalBank.Abbreviation.Should().Be("AWB"); // Unchanged

        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} validates duplicates for changed fields only")]
    public async Task PatchBank_Should_ValidateDuplicates_OnlyForChangedFields()
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

        var patchRequest = new PatchBankRequest
        {
            BankId = bankId,
            Code = "BMCE", // Attempting to change to existing code
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/banks/{bankId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} allows same values in unchanged fields")]
    public async Task PatchBank_Should_AllowSameValues_WhenNotChangingFields()
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

        var patchRequest = new PatchBankRequest
        {
            BankId = bankId,
            Code = "AWB", // Same code as current (should be allowed)
            Name = "New Name" // Different name
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/banks/{bankId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/banks/{id} returns 404 when bank not found")]
    public async Task PatchBank_Should_ReturnNotFound_WhenBankNotFound()
    {
        // Arrange
        var bankId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == bankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank)null);

        var patchRequest = new PatchBankRequest
        {
            BankId = bankId,
            Name = "New Name"
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/banks/{bankId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Never);
    }

    [Theory(DisplayName = "PATCH /api/banks/{id} updates individual fields correctly")]
    [InlineData("NewCode", null, null, null)]
    [InlineData(null, "NewName", null, null)]
    [InlineData(null, null, "NewAbbreviation", null)]
    [InlineData(null, null, null, false)]
    public async Task PatchBank_Should_UpdateIndividualFields_Correctly(
        string code, string name, string abbreviation, bool? isEnabled)
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

        var patchRequest = new PatchBankRequest
        {
            BankId = bankId,
            Code = code,
            Name = name,
            Abbreviation = abbreviation,
            IsEnabled = isEnabled
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/banks/{bankId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<Bank>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}