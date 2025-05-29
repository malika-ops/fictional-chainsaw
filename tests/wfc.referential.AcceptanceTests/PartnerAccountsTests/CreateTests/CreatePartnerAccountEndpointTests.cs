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
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.CreateTests;

public class CreatePartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();
    private readonly Mock<IBankRepository> _bankRepoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();

    public CreatePartnerAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICacheService>();

                // Updated to use BaseRepository methods
                _repoMock.Setup(r => r.AddAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerAccount p, CancellationToken _) => p);
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Set up bank mock to return valid entities
                var bankId = BankId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                _bankRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<BankId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Bank.Create(bankId, "AWB", "Attijariwafa Bank", "AWB"));

                // Set up param type mock to return valid account types
                var activityTypeId = ParamTypeId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
                var commissionTypeId = ParamTypeId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333"));

                var activityType = ParamType.Create(activityTypeId, null, "Activity");
                var commissionType = ParamType.Create(commissionTypeId, null, "Commission");

                _paramTypeRepoMock
                    .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(id => id.Value == activityTypeId.Value), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activityType);

                _paramTypeRepoMock
                    .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(id => id.Value == commissionTypeId.Value), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(commissionType);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_bankRepoMock.Object);
                services.AddSingleton(_paramTypeRepoMock.Object);
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
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            AccountNumber = "000123456789",
            RIB = "12345678901234567890123",
            Domiciliation = "Casablanca Centre",
            BusinessName = "Wafa Cash Services",
            ShortName = "WCS",
            AccountBalance = 50000.00m,
            BankId = bankId,
            AccountTypeId = accountTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<PartnerAccount>(p =>
                    p.AccountNumber == payload.AccountNumber &&
                    p.RIB == payload.RIB &&
                    p.BusinessName == payload.BusinessName &&
                    p.ShortName == payload.ShortName &&
                    p.AccountBalance == payload.AccountBalance &&
                    p.Bank.Id.Value == bankId &&
                    p.AccountTypeId.Value == accountTypeId),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/partner-accounts returns 409 when AccountNumber already exists")]
    public async Task Post_ShouldReturn409_WhenAccountNumberAlreadyExists()
    {
        // Arrange 
        const string duplicateAccountNumber = "000123456789";
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var existingBank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");
        var existingAccountType = ParamType.Create(ParamTypeId.Of(accountTypeId), null, "Activity");

        var existingAccount = PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            duplicateAccountNumber,
            "12345678901234567890123",
            "Casablanca Centre",
            "Existing Business",
            "EB",
            50000.00m,
            existingBank,
            existingAccountType
        );

        _repoMock
            .Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
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
            AccountTypeId = Guid.Parse("33333333-3333-3333-3333-333333333333")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner-accounts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r =>
            r.AddAsync(It.IsAny<PartnerAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}