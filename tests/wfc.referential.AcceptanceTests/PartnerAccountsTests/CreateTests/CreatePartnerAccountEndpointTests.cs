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
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.CreateTests;

public class CreatePartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();
    private readonly Mock<IBankRepository> _bankRepoMock = new();

    public CreatePartnerAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddPartnerAccountAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerAccount p, CancellationToken _) => p);

                // Set up bank mock to return valid entities
                var bankId = BankId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                _bankRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Bank.Create(bankId, "AWB", "Attijariwafa Bank", "AWB"));

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_bankRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/partner-accounts returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            AccountNumber = "000123456789",
            RIB = "12345678901234567890123",
            Domiciliation = "Casablanca Centre",
            BusinessName = "Wafa Cash Services",
            ShortName = "WCS",
            AccountBalance = 50000.00m,
            BankId = bankId,
            AccountType = "Activité"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _repoMock.Verify(r =>
            r.AddPartnerAccountAsync(It.Is<PartnerAccount>(p =>
                    p.AccountNumber == payload.AccountNumber &&
                    p.RIB == payload.RIB &&
                    p.BusinessName == payload.BusinessName &&
                    p.ShortName == payload.ShortName &&
                    p.AccountBalance == payload.AccountBalance &&
                    p.Bank.Id.Value == bankId &&
                    p.AccountType == AccountType.Activité),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/partner-accounts returns 400 & problem-details when AccountNumber is missing")]
    public async Task Post_ShouldReturn400_WhenAccountNumberIsMissing()
    {
        // Arrange
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var invalidPayload = new
        {
            // AccountNumber intentionally omitted to trigger validation error
            RIB = "12345678901234567890123",
            Domiciliation = "Casablanca Centre",
            BusinessName = "Wafa Cash Services",
            ShortName = "WCS",
            AccountBalance = 50000.00m,
            BankId = bankId,
            AccountType = "Activité"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("accountNumber")[0].GetString()
            .Should().Be("Account number is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddPartnerAccountAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/partner-accounts returns 400 when AccountNumber already exists")]
    public async Task Post_ShouldReturn400_WhenAccountNumberAlreadyExists()
    {
        // Arrange 
        const string duplicateAccountNumber = "000123456789";
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Tell the repo mock that the account number already exists
        var existingBank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        var existingAccount = PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            duplicateAccountNumber,
            "12345678901234567890123",
            "Casablanca Centre",
            "Existing Business",
            "EB",
            50000.00m,
            existingBank,
            AccountType.Activité
        );

        _repoMock
            .Setup(r => r.GetByAccountNumberAsync(duplicateAccountNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var payload = new
        {
            AccountNumber = duplicateAccountNumber,
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "Transfert Express",
            ShortName = "TE",
            AccountBalance = 75000.00m,
            BankId = bankId,
            AccountType = "Commission"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Partner account with account number {duplicateAccountNumber} already exists.");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddPartnerAccountAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the account number is already taken");
    }

    [Fact(DisplayName = "POST /api/partner-accounts returns 400 when Bank is not found")]
    public async Task Post_ShouldReturn400_WhenBankNotFound()
    {
        // Arrange
        var nonExistentBankId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Setup bank repository to return null for this ID
        _bankRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<BankId>(id => id.Value == nonExistentBankId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bank?)null);

        var payload = new
        {
            AccountNumber = "000987654321",
            RIB = "98765432109876543210987",
            Domiciliation = "Casablanca Marina",
            BusinessName = "Transfert Express",
            ShortName = "TE",
            AccountBalance = 75000.00m,
            BankId = nonExistentBankId,
            AccountType = "Commission"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Bank with ID {nonExistentBankId} not found");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddPartnerAccountAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}