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
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.UpdateTests;

public class UpdateSupportAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();

    public UpdateSupportAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var sectorId = Guid.NewGuid();
                var cityId = Guid.NewGuid();

                var partner = Partner.Create(
                    PartnerId.Of(partnerId),
                    "P001",
                    "Test Partner",
                    NetworkMode.Franchise,
                    PaymentMode.PrePaye,
                    "ID001",
                    SupportAccountType.Commun,
                    "IDNUM001",
                    "Standard",
                    "AUX001",
                    "ICE001",
                    "/logos/logo.png",
                    null, // IdParent
                    null, // CommissionAccountId
                    null, // ActivityAccountId
                    null  // SupportAccountId
                );

                _partnerRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(partner);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_partnerRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static SupportAccount CreateTestSupportAccount(Guid id, string code, string name, decimal threshold, decimal limit, decimal balance)
    {
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var sectorId = Guid.NewGuid();
        var cityId = Guid.NewGuid();


        var partner = Partner.Create(
            PartnerId.Of(partnerId),
            "P001",
            "Test Partner",
            NetworkMode.Franchise,
            PaymentMode.PrePaye,
            "ID001",
            SupportAccountType.Commun,
            "IDNUM001",
            "Standard",
            "AUX001",
            "ICE001",
            "/logos/logo.png",
            null, // IdParent
            null, // CommissionAccountId
            null, // ActivityAccountId
            null  // SupportAccountId
        );

        return SupportAccount.Create(
            new SupportAccountId(id),
            code,
            name,
            threshold,
            limit,
            balance,
            "ACC" + code,
            partner,
            SupportAccountType.Commun
        );
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var oldAccount = CreateTestSupportAccount(id, "SA001", "Old Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        _repoMock.Setup(r => r.GetByCodeAsync("SA002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount?)null);   // Code is unique

        _repoMock.Setup(r => r.GetByAccountingNumberAsync("ACC002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount?)null);   // Accounting number is unique

        SupportAccount? updated = null;
        _repoMock.Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<SupportAccount, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002",
            Name = "New Support Account",
            Threshold = 12000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = partnerId,
            SupportAccountType = "Individuel",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("SA002");
        updated.Name.Should().Be("New Support Account");
        updated.Threshold.Should().Be(12000.00m);
        updated.Limit.Should().Be(25000.00m);
        updated.AccountBalance.Should().Be(7500.00m);
        updated.AccountingNumber.Should().Be("ACC002");
        updated.SupportAccountType.Should().Be(SupportAccountType.Individuel);
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} allows changing only the Limit")]
    public async Task Put_ShouldAllowChangingOnlyLimit_WhenOtherFieldsUnchanged()
    {
        var id = Guid.NewGuid();
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var oldAccount = CreateTestSupportAccount(id, "SA001", "Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        _repoMock.Setup(r => r.GetByCodeAsync("SA001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same code is ok for same account

        _repoMock.Setup(r => r.GetByAccountingNumberAsync("ACCSA001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same accounting number is ok for same account

        SupportAccount? updated = null;
        _repoMock.Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<SupportAccount, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA001",
            Name = "Support Account",
            Threshold = 10000.00m,
            Limit = 25000.00m, // Only changed this from 15000 to 25000
            AccountBalance = 5000.00m,
            AccountingNumber = "ACCSA001",
            PartnerId = partnerId,
            SupportAccountType = "Commun",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Limit.Should().Be(25000.00m); // Should be updated
        updated.Code.Should().Be("SA001");
        updated.Name.Should().Be("Support Account");
        updated.Threshold.Should().Be(10000.00m);
        updated.AccountBalance.Should().Be(5000.00m);

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} allows changing the enabled status")]
    public async Task Put_ShouldAllowChangingEnabledStatus_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var oldAccount = CreateTestSupportAccount(id, "SA001", "Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);

        _repoMock.Setup(r => r.GetByCodeAsync("SA001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same code is ok for same account

        _repoMock.Setup(r => r.GetByAccountingNumberAsync("ACCSA001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);   // Same accounting number is ok for same account

        SupportAccount? updated = null;
        _repoMock.Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<SupportAccount, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA001",
            Name = "Support Account",
            Threshold = 10000.00m,
            Limit = 15000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACCSA001",
            PartnerId = partnerId,
            SupportAccountType = "Commun",
            IsEnabled = false // Changed from true to false
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 400 when Limit is negative")]
    public async Task Put_ShouldReturn400_WhenLimitIsNegative()
    {
        var id = Guid.NewGuid();
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA001",
            Name = "Support Account",
            Threshold = 10000.00m,
            Limit = -5000.00m, // Negative limit
            AccountBalance = 5000.00m,
            AccountingNumber = "ACCSA001",
            PartnerId = partnerId,
            SupportAccountType = "Commun",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("limit")[0].GetString()
            .Should().Be("Limit must be non-negative");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        var id = Guid.NewGuid();
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            SupportAccountId = id,
            // Code intentionally omitted
            Name = "New Support Account",
            Threshold = 15000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = partnerId,
            SupportAccountType = "Individuel",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 400 when account doesn't exist")]
    public async Task Put_ShouldReturn404_WhenAccountDoesNotExist()
    {
        var id = Guid.NewGuid();
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount?)null);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002",
            Name = "New Support Account",
            Threshold = 15000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = partnerId,
            SupportAccountType = "Individuel",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Support account with ID {id} not found");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}