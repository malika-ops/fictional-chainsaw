using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.UpdateBalanceTests;

public class UpdateBalanceEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
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

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance returns 200 and updates only the balance")]
    public async Task Patch_ShouldReturn200_AndUpdateOnlyBalance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account 1", 10000.00m, 20000.00m, 5000.00m);

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = 7500.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify balance was updated
        supportAccount.AccountBalance.Should().Be(7500.00m);
        // Verify other fields unchanged
        supportAccount.Code.Should().Be("SA001");
        supportAccount.Description.Should().Be("Support Account 1");
        supportAccount.Threshold.Should().Be(10000.00m);
        supportAccount.Limit.Should().Be(20000.00m);

        _supportAccountRepoMock.Verify(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance returns 400 when account doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount)null);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = 7500.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance returns 400 when balance is negative")]
    public async Task Patch_ShouldReturn400_WhenBalanceIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account 1", 10000.00m, 20000.00m, 5000.00m);

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = -1000.00m // Negative balance
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance doesn't affect other field values")]
    public async Task Patch_ShouldNotAffectOtherFields_WhenUpdatingBalance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalLimit = 20000.00m;
        var originalThreshold = 10000.00m;
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account 1", originalThreshold, originalLimit, 5000.00m);

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = 10000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        supportAccount.Limit.Should().Be(originalLimit); // Limit should remain unchanged
        supportAccount.Threshold.Should().Be(originalThreshold); // Threshold should remain unchanged
        supportAccount.AccountBalance.Should().Be(10000.00m);  // Balance should change

        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance validates support account exists before update")]
    public async Task Patch_ShouldValidateSupportAccountExists_BeforeUpdate()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(nonExistentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportAccount)null);

        var payload = new
        {
            SupportAccountId = nonExistentId,
            NewBalance = 5000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{nonExistentId}/balance", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}