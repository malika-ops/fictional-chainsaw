using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.DeleteTests;

public class DeletePartnerEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 200 when partner exists and has no support accounts")]
    public async Task Delete_ShouldReturn200_WhenPartnerExistsAndHasNoSupportAccounts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount>()); // No support accounts associated

        // Capture the entity passed to SaveChanges
        Partner? updatedPartner = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => updatedPartner = partner)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();
        updatedPartner!.IsEnabled.Should().BeFalse();
        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 400 when partner is not found")]
    public async Task Delete_ShouldReturn400_WhenPartnerNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Partner not found");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 400 when partner has support accounts")]
    public async Task Delete_ShouldReturn400_WhenPartnerHasSupportAccounts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Mock that partner has support accounts
        var supportAccount = SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            "SA001",
            "Support Account",
            10000.00m,
            20000.00m,
            5000.00m,
            "ACC001",
            SupportAccountTypeEnum.Individuel
        );
        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(
                It.IsAny<Expression<Func<SupportAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount> { supportAccount });

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Cannot delete partner with existing support accounts");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} performs soft delete instead of physical deletion")]
    public async Task Delete_ShouldPerformSoftDelete_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        // Verify partner starts as enabled
        partner.IsEnabled.Should().BeTrue();

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount>());

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        partner.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (partner object still exists)
        partner.Should().NotBeNull();
        partner.Code.Should().Be("PTN001"); // Data still intact
        partner.Name.Should().Be("Test Partner");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/partners/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _partnerRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()), Times.Never);
        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} validates business rules before deletion")]
    public async Task Delete_ShouldValidateBusinessRules_BeforeDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Setup support account check to return empty list (no blocking accounts)
        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount>());

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify business rule validation was performed
        _supportAccountRepoMock.Verify(r => r.GetByConditionAsync(
            It.IsAny<Expression<Func<SupportAccount, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify partner was disabled (soft delete)
        partner.IsEnabled.Should().BeFalse();
        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} handles multiple support accounts scenario")]
    public async Task Delete_ShouldReturn400_WhenPartnerHasMultipleSupportAccounts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Mock multiple support accounts
        var supportAccounts = new List<SupportAccount>
        {
            SupportAccount.Create(SupportAccountId.Of(Guid.NewGuid()), "SA001", "Support Account 1", 1000m, 5000m, 2000m, "ACC001",SupportAccountTypeEnum.Individuel),
            SupportAccount.Create(SupportAccountId.Of(Guid.NewGuid()), "SA002", "Support Account 2", 1500m, 7500m, 3000m, "ACC002",SupportAccountTypeEnum.Individuel),
            SupportAccount.Create(SupportAccountId.Of(Guid.NewGuid()), "SA003", "Support Account 3", 2000m, 10000m, 4000m, "ACC003", SupportAccountTypeEnum.Individuel)
        };

        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(
                It.IsAny<Expression<Func<SupportAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportAccounts);

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Cannot delete partner with existing support accounts");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} verifies partner state before and after deletion")]
    public async Task Delete_ShouldVerifyPartnerStateBeforeAndAfterDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Active Partner");

        // Ensure partner starts enabled
        partner.IsEnabled.Should().BeTrue("Partner should start as enabled");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount>());

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify final state - partner should be disabled but data intact
        partner.IsEnabled.Should().BeFalse("Partner should be disabled after deletion");
        partner.Code.Should().Be("PTN001", "Partner code should remain intact");
        partner.Name.Should().Be("Active Partner", "Partner name should remain intact");
        partner.PersonType.Should().Be("Natural Person", "Partner person type should remain intact");

        // Verify repository interactions
        _partnerRepoMock.Verify(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
        _supportAccountRepoMock.Verify(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} handles empty GUID correctly")]
    public async Task Delete_ShouldReturn400_ForEmptyGuid()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/partners/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("PartnerId must be a non-empty GUID");

        // Verify no repository operations were attempted
        _partnerRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()), Times.Never);
        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} ensures atomic operation")]
    public async Task Delete_ShouldEnsureAtomicOperation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount>());

        // Track the sequence of operations
        var operationSequence = new List<string>();

        _partnerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("GetById"))
            .ReturnsAsync(partner);

        _supportAccountRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("CheckSupportAccounts"))
            .ReturnsAsync(new List<SupportAccount>());

        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify operation sequence
        operationSequence.Should().ContainInOrder("GetById", "CheckSupportAccounts", "SaveChanges");
        operationSequence.Should().HaveCount(3);

        // Verify all operations were called exactly once
        _partnerRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()), Times.Once);
        _supportAccountRepoMock.Verify(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper to build dummy partners quickly
    private static Partner CreateTestPartner(Guid id, string code, string name)
    {
        return Partner.Create(
            PartnerId.Of(id),
            code,
            name,
            "Natural Person",
            "PTX123456",
            "10.5",
            "Casablanca",
            "123 Main Street",
            "Doe",
            "John",
            "+212612345678",
            "contact@partner.com",
            "Manager",
            "Bank Transfer",
            "SMS",
            "TAX123456",
            "Standard",
            "AUX001",
            "ICE123456789",
            "/logos/logo.png"
        );
    }
}