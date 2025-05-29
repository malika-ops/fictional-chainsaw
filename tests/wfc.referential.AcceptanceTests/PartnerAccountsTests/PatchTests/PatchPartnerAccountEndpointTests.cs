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

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.PatchTests;

public class PatchPartnerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerAccountRepository> _repoMock = new();
    private readonly Mock<IBankRepository> _bankRepoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();

    public PatchPartnerAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<IBankRepository>();
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICacheService>();

                // Updated to use BaseRepository methods
                _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()));
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

        _client = customisedFactory.CreateClient();
    }

    private static PartnerAccount CreateTestPartnerAccount(Guid id, string accountNumber, string rib, string businessName)
    {
        var bankId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        var accountTypeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var accountType = ParamType.Create(ParamTypeId.Of(accountTypeId), null, "Activity");

        return PartnerAccount.Create(
            PartnerAccountId.Of(id),
            accountNumber,
            rib,
            "Casablanca Centre",
            businessName,
            businessName.Substring(0, 2).ToUpper(),
            50000.00m,
            bank,
            accountType
        );
    }

    [Fact(DisplayName = "PATCH /api/partner-accounts/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerAccount = CreateTestPartnerAccount(id, "000123456789", "12345678901234567890123", "Old Business");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerAccountId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partnerAccount);

        _repoMock.Setup(r => r.GetOneByConditionAsync(
            It.Is<System.Linq.Expressions.Expression<System.Func<PartnerAccount, bool>>>(
                expr => expr.Compile().Invoke(partnerAccount) == false), // Different account number
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerAccount?)null);

        PartnerAccount? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<PartnerAccount>()))
                 .Callback<PartnerAccount>(p => updated = p);

        var payload = new
        {
            PartnerAccountId = id,
            AccountNumber = "000987654321",
            AccountBalance = 75000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/partner-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.AccountNumber.Should().Be("000987654321");
        updated.AccountBalance.Should().Be(75000.00m);
        updated.RIB.Should().Be("12345678901234567890123");
        updated.BusinessName.Should().Be("Old Business");

        _repoMock.Verify(r => r.Update(It.IsAny<PartnerAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}