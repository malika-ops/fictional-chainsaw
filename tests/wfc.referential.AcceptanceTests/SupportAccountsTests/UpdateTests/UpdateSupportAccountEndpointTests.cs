using System.Net;
using System.Net.Http.Json;
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

    public UpdateSupportAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();

                _repoMock.Setup(r => r.Update(It.IsAny<SupportAccount>()));
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
            });
        });
        _client = customisedFactory.CreateClient();
    }

    private static SupportAccount CreateTestSupportAccount(Guid id, string code, string description, decimal threshold, decimal limit, decimal balance)
    {
        return SupportAccount.Create(
            SupportAccountId.Of(id),
            code,
            description,
            threshold,
            limit,
            balance,
            "ACC" + code
        );
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var oldAccount = CreateTestSupportAccount(id, "SA001", "Old Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAccount);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount)null);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002",
            Description = "New Support Account",
            Threshold = 12000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 400 when account doesn't exist")]
    public async Task Put_ShouldReturn400_WhenAccountDoesNotExist()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount)null);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002",
            Description = "New Support Account",
            Threshold = 15000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/support-accounts/{id} returns 409 when Code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetAccount = CreateTestSupportAccount(id, "SA001", "Target Account", 10000.00m, 15000.00m, 5000.00m);
        var conflictingAccount = CreateTestSupportAccount(existingId, "SA002", "Existing Account", 12000.00m, 18000.00m, 6000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(targetAccount);
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(conflictingAccount);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002", // Code already exists
            Description = "Updated Account",
            Threshold = 15000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/support-accounts/{id}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.Update(It.IsAny<SupportAccount>()), Times.Never);
    }
}