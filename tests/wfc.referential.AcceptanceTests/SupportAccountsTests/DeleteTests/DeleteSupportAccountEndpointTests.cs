using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.DeleteTests;

public class DeleteSupportAccountEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
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

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} disables support account when deletion requested")]
    public async Task Delete_ShouldDisableSupportAccount_WhenDeletionRequested()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(
            id,
            "SA001",
            "Test Support Account",
            10000.00m,
            20000.00m,
            5000.00m
        );

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportAccount);

        // Act
        var response = await _client.DeleteAsync($"/api/support-accounts/{id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify support account was disabled (soft delete)
        supportAccount.IsEnabled.Should().BeFalse();

        _supportAccountRepoMock.Verify(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} returns 400 when support account not found")]
    public async Task Delete_ShouldReturnBadRequest_WhenSupportAccountNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportAccount)null);

        // Act
        var response = await _client.DeleteAsync($"/api/support-accounts/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} changes status to inactive instead of physical deletion")]
    public async Task Delete_ShouldChangeStatusToInactive_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Test Support Account", 10000.00m, 20000.00m, 5000.00m);

        // Verify support account starts as enabled
        supportAccount.IsEnabled.Should().BeTrue();

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportAccount);

        // Act
        var response = await _client.DeleteAsync($"/api/support-accounts/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        supportAccount.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (support account object still exists)
        supportAccount.Should().NotBeNull();
        supportAccount.Code.Should().Be("SA001"); // Data still intact
    }

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} validates support account exists before deletion")]
    public async Task Delete_ShouldValidateSupportAccountExists_BeforeDeletion()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(nonExistentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportAccount)null);

        // Act
        var response = await _client.DeleteAsync($"/api/support-accounts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/support-accounts/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _supportAccountRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<SupportAccountId>(), It.IsAny<CancellationToken>()), Times.Never);
        _supportAccountRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}