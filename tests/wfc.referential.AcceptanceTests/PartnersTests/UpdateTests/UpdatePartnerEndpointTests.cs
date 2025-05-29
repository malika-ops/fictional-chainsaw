using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;
using System.Text.Json;

namespace wfc.referential.AcceptanceTests.PartnersTests.UpdateTests;

public class UpdatePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();
    private readonly Mock<IPartnerAccountRepository> _partnerAccountRepoMock = new();
    private readonly Mock<ISupportAccountRepository> _supportAccountRepoMock = new();

    public UpdatePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Setup repository mocks
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Setup ParamType repository to return valid entities
                _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType());

                _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerAccountId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerAccountId id, CancellationToken _) => CreateMockPartnerAccount());

                _supportAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<SupportAccountId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((SupportAccountId id, CancellationToken _) => CreateMockSupportAccount());

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_paramTypeRepoMock.Object);
                services.AddSingleton(_partnerAccountRepoMock.Object);
                services.AddSingleton(_supportAccountRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 200 when update succeeds with all fields")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessfulWithAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldPartner = CreateTestPartner(id, "PTN001", "Old Partner");

        // Create specific GUIDs for foreign keys
        var networkModeId = Guid.NewGuid();
        var paymentModeId = Guid.NewGuid();
        var partnerTypeId = Guid.NewGuid();
        var supportAccountTypeId = Guid.NewGuid();
        var commissionAccountId = Guid.NewGuid();
        var activityAccountId = Guid.NewGuid();
        var supportAccountId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>()); // No conflicts

        // Setup parent partner mock
        var parentPartner = CreateTestPartner(parentId, "PTN_PARENT", "Parent Partner");
        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(parentId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(parentPartner);

        // Setup ParamType mocks for specific IDs
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(networkModeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(CreateMockParamType());

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(paymentModeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(CreateMockParamType());

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(partnerTypeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(CreateMockParamType());

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(supportAccountTypeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(CreateMockParamType());

        // Setup PartnerAccount mocks for specific IDs
        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(PartnerAccountId.Of(commissionAccountId), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(CreateMockPartnerAccount());

        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(PartnerAccountId.Of(activityAccountId), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(CreateMockPartnerAccount());

        // Setup SupportAccount mock for specific ID
        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(supportAccountId), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(CreateMockSupportAccount());

        Partner? updated = null;
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = oldPartner)
                 .Returns(Task.CompletedTask);

        var payload = CreateCompleteUpdatePayloadWithSpecificIds(id, networkModeId, paymentModeId,
            partnerTypeId, supportAccountTypeId, commissionAccountId, activityAccountId,
            supportAccountId, parentId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Debug output if test fails
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: Status {response.StatusCode}: {errorContent}");
        }

        // Read the response as boolean
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.Code.Should().Be("PTN002");
        updated.Name.Should().Be("New Partner Name");
        updated.PersonType.Should().Be("Legal Person");
        updated.ProfessionalTaxNumber.Should().Be("PTX654321");
        updated.WithholdingTaxRate.Should().Be("12.5");
        updated.HeadquartersCity.Should().Be("Rabat");
        updated.HeadquartersAddress.Should().Be("456 New Avenue");
        updated.LastName.Should().Be("Smith");
        updated.FirstName.Should().Be("Jane");
        updated.PhoneNumberContact.Should().Be("+212687654321");
        updated.MailContact.Should().Be("newcontact@partner.com");
        updated.FunctionContact.Should().Be("Director");
        updated.TransferType.Should().Be("Wire Transfer");
        updated.AuthenticationMode.Should().Be("Email");
        updated.TaxIdentificationNumber.Should().Be("TAX654321");
        updated.TaxRegime.Should().Be("Simplified");
        updated.AuxiliaryAccount.Should().Be("AUX002");
        updated.ICE.Should().Be("ICE987654321");
        updated.Logo.Should().Be("/logos/partner002.png");
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper method to create payload with specific IDs
    private static object CreateCompleteUpdatePayloadWithSpecificIds(
        Guid id,
        Guid networkModeId,
        Guid paymentModeId,
        Guid partnerTypeId,
        Guid supportAccountTypeId,
        Guid commissionAccountId,
        Guid activityAccountId,
        Guid supportAccountId,
        Guid parentId)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN002",
            Name = "New Partner Name",
            PersonType = "Legal Person",
            ProfessionalTaxNumber = "PTX654321",
            WithholdingTaxRate = "12.5",
            HeadquartersCity = "Rabat",
            HeadquartersAddress = "456 New Avenue",
            LastName = "Smith",
            FirstName = "Jane",
            PhoneNumberContact = "+212687654321",
            MailContact = "newcontact@partner.com",
            FunctionContact = "Director",
            TransferType = "Wire Transfer",
            AuthenticationMode = "Email",
            TaxIdentificationNumber = "TAX654321",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE987654321",
            Logo = "/logos/partner002.png",
            IsEnabled = true,
            NetworkModeId = networkModeId,
            PaymentModeId = paymentModeId,
            PartnerTypeId = partnerTypeId,
            SupportAccountTypeId = supportAccountTypeId,
            CommissionAccountId = commissionAccountId,
            ActivityAccountId = activityAccountId,
            SupportAccountId = supportAccountId,
            IdParent = parentId
        };
    }


    [Fact(DisplayName = "PUT /api/partners/{id} validates NetworkMode exists")]
    public async Task Put_ShouldReturn400_WhenNetworkModeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        var networkModeId = Guid.NewGuid();
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(networkModeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ParamType?)null); // Not found

        var payload = CreateBasicUpdatePayloadWithNetworkMode(id, networkModeId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Debug output if test fails
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Unexpected status {response.StatusCode}: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Network Mode with ID {networkModeId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} validates PaymentMode exists")]
    public async Task Put_ShouldReturn400_WhenPaymentModeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        var paymentModeId = Guid.NewGuid();
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(paymentModeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ParamType?)null); // Not found

        var payload = CreateBasicUpdatePayloadWithPaymentMode(id, paymentModeId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Payment Mode with ID {paymentModeId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} validates CommissionAccount exists")]
    public async Task Put_ShouldReturn400_WhenCommissionAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        var commissionAccountId = Guid.NewGuid();
        _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(PartnerAccountId.Of(commissionAccountId), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((PartnerAccount?)null); // Not found

        var payload = CreateBasicUpdatePayloadWithCommissionAccount(id, commissionAccountId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Commission Account with ID {commissionAccountId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} validates SupportAccount exists")]
    public async Task Put_ShouldReturn400_WhenSupportAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        var supportAccountId = Guid.NewGuid();
        _supportAccountRepoMock.Setup(r => r.GetByIdAsync(SupportAccountId.Of(supportAccountId), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((SupportAccount?)null); // Not found

        var payload = CreateBasicUpdatePayloadWithSupportAccount(id, supportAccountId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Support Account with ID {supportAccountId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} validates Parent Partner exists")]
    public async Task Put_ShouldReturn400_WhenParentPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner");

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        var parentId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(parentId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null); // Parent not found

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner>());

        var payload = CreateBasicUpdatePayloadWithParent(id, parentId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Parent Partner with ID {parentId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            PartnerId = id,
            // Code intentionally omitted
            Name = "New Partner Name",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 409 when Code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetPartner = CreateTestPartner(id, "PTN001", "Target Partner");
        var conflictingPartner = CreateTestPartner(existingId, "PTN002", "Existing Partner");

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(targetPartner);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Partner> { conflictingPartner });

        var payload = CreateBasicUpdatePayloadWithCode(id, "PTN002");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 400 when partner doesn't exist")]
    public async Task Put_ShouldReturn400_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);

        var payload = CreateBasicUpdatePayload(id);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner [{id}] not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper methods - Using strongly typed anonymous objects instead of dynamic
    private static object CreateCompleteUpdatePayload(Guid id)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN002",
            Name = "New Partner Name",
            PersonType = "Legal Person",
            ProfessionalTaxNumber = "PTX654321",
            WithholdingTaxRate = "12.5",
            HeadquartersCity = "Rabat",
            HeadquartersAddress = "456 New Avenue",
            LastName = "Smith",
            FirstName = "Jane",
            PhoneNumberContact = "+212687654321",
            MailContact = "newcontact@partner.com",
            FunctionContact = "Director",
            TransferType = "Wire Transfer",
            AuthenticationMode = "Email",
            TaxIdentificationNumber = "TAX654321",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE987654321",
            Logo = "/logos/partner002.png",
            IsEnabled = true,
            NetworkModeId = Guid.NewGuid(),
            PaymentModeId = Guid.NewGuid(),
            PartnerTypeId = Guid.NewGuid(),
            SupportAccountTypeId = Guid.NewGuid(),
            CommissionAccountId = Guid.NewGuid(),
            ActivityAccountId = Guid.NewGuid(),
            SupportAccountId = Guid.NewGuid(),
            IdParent = Guid.NewGuid()
        };
    }

    private static object CreateBasicUpdatePayload(Guid id)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN001",
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = (Guid?)null,
            PaymentModeId = (Guid?)null,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IdParent = (Guid?)null
        };
    }

    private static object CreateBasicUpdatePayloadWithNetworkMode(Guid id, Guid networkModeId)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN001",
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = networkModeId,
            PaymentModeId = (Guid?)null,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IdParent = (Guid?)null
        };
    }

    private static object CreateBasicUpdatePayloadWithPaymentMode(Guid id, Guid paymentModeId)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN001",
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = (Guid?)null,
            PaymentModeId = paymentModeId,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IdParent = (Guid?)null
        };
    }

    private static object CreateBasicUpdatePayloadWithCommissionAccount(Guid id, Guid commissionAccountId)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN001",
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = (Guid?)null,
            PaymentModeId = (Guid?)null,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = commissionAccountId,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IdParent = (Guid?)null
        };
    }

    private static object CreateBasicUpdatePayloadWithSupportAccount(Guid id, Guid supportAccountId)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN001",
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = (Guid?)null,
            PaymentModeId = (Guid?)null,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = supportAccountId,
            IdParent = (Guid?)null
        };
    }

    private static object CreateBasicUpdatePayloadWithParent(Guid id, Guid parentId)
    {
        return new
        {
            PartnerId = id,
            Code = "PTN001",
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = (Guid?)null,
            PaymentModeId = (Guid?)null,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IdParent = parentId
        };
    }

    private static object CreateBasicUpdatePayloadWithCode(Guid id, string code)
    {
        return new
        {
            PartnerId = id,
            Code = code,
            Name = "Test Partner",
            PersonType = "Natural Person",
            ProfessionalTaxNumber = "PTX123456",
            WithholdingTaxRate = "10.5",
            HeadquartersCity = "Casablanca",
            HeadquartersAddress = "123 Main Street",
            LastName = "Doe",
            FirstName = "John",
            PhoneNumberContact = "+212612345678",
            MailContact = "contact@partner.com",
            FunctionContact = "Manager",
            TransferType = "Bank Transfer",
            AuthenticationMode = "SMS",
            TaxIdentificationNumber = "TAX123456",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE123456789",
            Logo = "/logos/logo.png",
            IsEnabled = true,
            NetworkModeId = (Guid?)null,
            PaymentModeId = (Guid?)null,
            PartnerTypeId = (Guid?)null,
            SupportAccountTypeId = (Guid?)null,
            CommissionAccountId = (Guid?)null,
            ActivityAccountId = (Guid?)null,
            SupportAccountId = (Guid?)null,
            IdParent = (Guid?)null
        };
    }

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
            "ACC123456");
    }
}