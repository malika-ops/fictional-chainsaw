using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.PatchTests;

public class PatchPartnerEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Old Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _partnerRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>()); // No conflicts

        Partner? updated = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = partner)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002",
            Name = "New Partner Name"
            // Other fields intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.Code.Should().Be("PTN002");  // Should change
        updated.Name.Should().Be("New Partner Name");  // Should change
        updated.PersonType.Should().Be("Natural Person"); // Should not change
        updated.ProfessionalTaxNumber.Should().Be("PTX123456"); // Should not change
        updated.HeadquartersCity.Should().Be("Casablanca"); // Should not change
        updated.IsEnabled.Should().BeTrue(); // Should remain enabled

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} allows changing only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        Partner? updated = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = partner)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            IsEnabled = false // Change from enabled to disabled
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.IsEnabled.Should().BeFalse(); // Should be disabled
        updated.Code.Should().Be("PTN001"); // Should not change
        updated.Name.Should().Be("Test Partner"); // Should not change

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} allows updating contact information")]
    public async Task Patch_ShouldAllowUpdatingContactInformation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _partnerRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        Partner? updated = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = partner)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            LastName = "Smith",
            FirstName = "Jane",
            PhoneNumberContact = "+212687654321",
            MailContact = "jane.smith@partner.com",
            FunctionContact = "Director"
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.LastName.Should().Be("Smith");
        updated.FirstName.Should().Be("Jane");
        updated.PhoneNumberContact.Should().Be("+212687654321");
        updated.MailContact.Should().Be("jane.smith@partner.com");
        updated.FunctionContact.Should().Be("Director");

        // Other fields should remain unchanged
        updated.Code.Should().Be("PTN001");
        updated.Name.Should().Be("Test Partner");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} allows updating headquarters information")]
    public async Task Patch_ShouldAllowUpdatingHeadquartersInformation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _partnerRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        Partner? updated = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = partner)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            HeadquartersCity = "Rabat",
            HeadquartersAddress = "456 Government Avenue"
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.HeadquartersCity.Should().Be("Rabat");
        updated.HeadquartersAddress.Should().Be("456 Government Avenue");

        // Other fields should remain unchanged
        updated.Code.Should().Be("PTN001");
        updated.Name.Should().Be("Test Partner");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} allows updating relationship IDs")]
    public async Task Patch_ShouldAllowUpdatingRelationshipIds()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType());

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerAccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerAccountId id, CancellationToken _) => CreateMockPartnerAccount());

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<SupportAccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportAccountId id, CancellationToken _) => CreateMockSupportAccount());

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _partnerRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        Partner? updated = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = partner)
                 .Returns(Task.CompletedTask);

        var networkModeId = Guid.NewGuid();
        var paymentModeId = Guid.NewGuid();
        var supportAccountId = Guid.NewGuid();

        var payload = new
        {
            PartnerId = id,
            NetworkModeId = networkModeId,
            PaymentModeId = paymentModeId,
            SupportAccountId = supportAccountId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        // Verify validation calls were made
        _paramTypeRepoMock.Verify(r => r.GetByIdAsync(ParamTypeId.Of(networkModeId), It.IsAny<CancellationToken>()), Times.Once);
        _paramTypeRepoMock.Verify(r => r.GetByIdAsync(ParamTypeId.Of(paymentModeId), It.IsAny<CancellationToken>()), Times.Once);
        _supportAccountRepoMock.Verify(r => r.GetByIdAsync(SupportAccountId.Of(supportAccountId), It.IsAny<CancellationToken>()), Times.Once);

        // Other fields should remain unchanged
        updated!.Code.Should().Be("PTN001");
        updated.Name.Should().Be("Test Partner");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} validates NetworkMode exists when provided")]
    public async Task Patch_ShouldReturn400_WhenNetworkModeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        var networkModeId = Guid.NewGuid();
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(networkModeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ParamType?)null); // Not found

        var payload = new
        {
            PartnerId = id,
            NetworkModeId = networkModeId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Network Mode with ID {networkModeId} not found");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} validates PartnerType exists when provided")]
    public async Task Patch_ShouldReturn400_WhenPartnerTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        var partnerTypeId = Guid.NewGuid();
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(partnerTypeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ParamType?)null); // Not found

        var payload = new
        {
            PartnerId = id,
            PartnerTypeId = partnerTypeId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner Type with ID {partnerTypeId} not found");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} validates ActivityAccount exists when provided")]
    public async Task Patch_ShouldReturn400_WhenActivityAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        var activityAccountId = Guid.NewGuid();
        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(PartnerAccountId.Of(activityAccountId), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((PartnerAccount?)null); // Not found

        var payload = new
        {
            PartnerId = id,
            ActivityAccountId = activityAccountId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Activity Account with ID {activityAccountId} not found");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} validates Parent Partner exists when provided")]
    public async Task Patch_ShouldReturn400_WhenParentPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        var parentId = Guid.NewGuid();
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(parentId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null); // Parent not found

        var payload = new
        {
            PartnerId = id,
            IdParent = parentId
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Parent Partner with ID {parentId} not found");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 400 when partner doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);

        var payload = new
        {
            PartnerId = id,
            Name = "New Partner Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner [{id}] not found");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 409 when new code already exists")]
    public async Task Patch_ShouldReturn409_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestPartner(existingId, "PTN002", "Existing Partner");
        var target = CreateTestPartner(id, "PTN001", "Target Partner");

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _partnerRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner> { existing }); // Duplicate code

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002"  // This code already exists for another partner
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Partner with code PTN002 already exists");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 400 when empty code provided")]
    public async Task Patch_ShouldReturn400_WhenEmptyCodeProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            PartnerId = id,
            Code = "" // Empty code should be invalid
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot be empty if provided");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} allows partial update with mixed field types")]
    public async Task Patch_ShouldAllowPartialUpdateWithMixedFieldTypes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType());

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerAccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerAccountId id, CancellationToken _) => CreateMockPartnerAccount());

        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<SupportAccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportAccountId id, CancellationToken _) => CreateMockSupportAccount());

        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _partnerRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        Partner? updated = null;
        _partnerRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = partner)
                 .Returns(Task.CompletedTask);

        var supportAccountTypeId = Guid.NewGuid();

        var payload = new
        {
            PartnerId = id,
            PersonType = "Legal Person", // String field
            IsEnabled = false, // Boolean field
            SupportAccountTypeId = supportAccountTypeId, // Guid field
            WithholdingTaxRate = "15.0" // String field
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        // Verify changed fields
        updated!.PersonType.Should().Be("Legal Person");
        updated.IsEnabled.Should().BeFalse();
        updated.WithholdingTaxRate.Should().Be("15.0");

        // Verify unchanged fields
        updated.Code.Should().Be("PTN001");
        updated.Name.Should().Be("Test Partner");
        updated.HeadquartersCity.Should().Be("Casablanca");

        _partnerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper to create a test partner
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

    // Mock entity creation methods
    private static ParamType CreateMockParamType()
    {
        return ParamType.Create(
            ParamTypeId.Of(Guid.NewGuid()),
            TypeDefinitionId.Of(Guid.NewGuid()),
            "Mock ParamType Value");
    }

    private static PartnerAccount CreateMockPartnerAccount()
    {
        var mockBank = Bank.Create(
            BankId.Of(Guid.NewGuid()),
            "Mock Bank",
            "MOCK_BANK",
            "Mock Bank Description");

        var mockAccountType = ParamType.Create(
            ParamTypeId.Of(Guid.NewGuid()),
            TypeDefinitionId.Of(Guid.NewGuid()),
            "Mock Account Type");

        return PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            "1234567890",
            "RIB123456789",
            "Mock Domiciliation",
            "Mock Business Name",
            "Mock Short",
            1000.00m,
            mockBank,
            mockAccountType);
    }

    private static SupportAccount CreateMockSupportAccount()
    {
        return SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            "SUPPORT001",
            "Mock Support Account",
            500.00m,
            10000.00m,
            2000.00m,
            "ACC123456", 
            SupportAccountTypeEnum.Individuel);
    }
}