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
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.CreateTests;

public class CreatePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();
    private readonly Mock<IPartnerAccountRepository> _partnerAccountRepoMock = new();
    private readonly Mock<ISupportAccountRepository> _supportAccountRepoMock = new();

    public CreatePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove all the services we want to mock
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Setup Partner Repository
                _repoMock.Setup(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Partner p, CancellationToken _) => p);
                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Partner>());
                _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Partner?)null);

                // Setup all other repositories to return valid entities
                _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType());

                _partnerAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerAccountId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerAccountId id, CancellationToken _) => CreateMockPartnerAccount());

                _supportAccountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<SupportAccountId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((SupportAccountId id, CancellationToken _) => CreateMockSupportAccount());

                // Register mocked services
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_paramTypeRepoMock.Object);
                services.AddSingleton(_partnerAccountRepoMock.Object);
                services.AddSingleton(_supportAccountRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/partners returns 200 and Guid when all required fields are provided")]
    public async Task Post_ShouldReturn200_AndId_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        Partner capturedCreatePartner = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
            .Callback<Partner, CancellationToken>((p, _) => capturedCreatePartner = p)
            .ReturnsAsync((Partner p, CancellationToken _) => p);

        var payload = CreateCompleteValidPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);

        // Debug output if test fails
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedCreatePartner.Should().NotBeNull();
        capturedCreatePartner.Code.Should().Be((string)payload["Code"]);
        capturedCreatePartner.Name.Should().Be((string)payload["Name"]);
        capturedCreatePartner.PersonType.Should().Be((string)payload["PersonType"]);
        capturedCreatePartner.IsEnabled.Should().BeTrue(); // IsEnabled = true by default
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("Code");

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when Name is missing")]
    public async Task Post_ShouldReturn400_WhenNameIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("Name");

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Name is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when PersonType is missing")]
    public async Task Post_ShouldReturn400_WhenPersonTypeIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("PersonType");

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("PersonType is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when TaxIdentificationNumber is missing")]
    public async Task Post_ShouldReturn400_WhenTaxIdentificationNumberIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["TaxIdentificationNumber"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Tax Identification Number is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when ICE is missing")]
    public async Task Post_ShouldReturn400_WhenICEIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["ICE"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ICE is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partners returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        const string duplicateCode = "PTN001";

        var existingPartner = CreateTestPartner(duplicateCode, "Existing Partner");
        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Partner> { existingPartner });

        var payload = CreateCompleteValidPayload();
        payload["Code"] = duplicateCode;

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);

        // Debug output
        if (response.StatusCode != HttpStatusCode.Conflict)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Expected 409, got {response.StatusCode}: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner with code {duplicateCode} already exists");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when TaxIdentificationNumber already exists")]
    public async Task Post_ShouldReturn400_WhenTaxIdentificationNumberAlreadyExists()
    {
        // Arrange
        const string duplicateTaxId = "TAX123456";

        _repoMock.SetupSequence(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Partner>()) // Code check - no duplicates
            .ReturnsAsync(new List<Partner> { CreateTestPartner("PTN002", "Existing Partner") }); // Tax ID check - duplicate found

        var payload = CreateCompleteValidPayload();
        payload["TaxIdentificationNumber"] = duplicateTaxId;

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner with tax identification number {duplicateTaxId} already exists");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "POST /api/partners accepts valid person types")]
    [InlineData("Natural Person")]
    [InlineData("Legal Person")]
    [InlineData("Natural Person/Legal Person")]
    public async Task Post_ShouldAcceptValidPersonTypes(string personType)
    {
        // Arrange
        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Partner>());

        var payload = CreateCompleteValidPayload();
        payload["PersonType"] = personType;

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);

        // Debug output if test fails
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error for {personType}: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/partners validates WithholdingTaxRate for Natural Person")]
    public async Task Post_ShouldValidateWithholdingTaxRateForNaturalPerson()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["PersonType"] = "Natural Person";
        invalidPayload["WithholdingTaxRate"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Withholding Tax Rate is required");
    }

    [Fact(DisplayName = "POST /api/partners validates all contact fields are required")]
    public async Task Post_ShouldValidateAllContactFieldsAreRequired()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["LastName"] = "";
        invalidPayload["FirstName"] = "";
        invalidPayload["PhoneNumberContact"] = "";
        invalidPayload["MailContact"] = "";
        invalidPayload["FunctionContact"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Last Name Contact is required");
        responseContent.Should().Contain("First Name Contact is required");
        responseContent.Should().Contain("Phone Number contact is required");
        responseContent.Should().Contain("Mail contact is required");
        responseContent.Should().Contain("Function contact is required");
    }

    [Fact(DisplayName = "POST /api/partners validates business rules for réseau propre")]
    public async Task Post_ShouldValidateBusinessRulesForReseauPropre()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["PaymentModeId"] = null;
        invalidPayload["TransferType"] = "";
        invalidPayload["ActivityAccountId"] = null;
        invalidPayload["CommissionAccountId"] = null;

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        // Check for the actual error messages from the validator
        responseContent.Should().Contain("Payment Mode is required when PartnerType");
        responseContent.Should().Contain("Transfer Type is required when PartnerType");
        responseContent.Should().Contain("Activity Account is required when PartnerType");
        responseContent.Should().Contain("Commission Account is required when PartnerType");
    }

    [Fact(DisplayName = "POST /api/partners validates business rules for prépayé")]
    public async Task Post_ShouldValidateBusinessRulesForPrepaye()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["SupportAccountTypeId"] = null;
        invalidPayload["SupportAccountId"] = null;

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        // Check for the actual error messages from the validator
        responseContent.Should().Contain("Support Account Type is required when Network mode");
        responseContent.Should().Contain("Support Account ID is required when Network mode");
    }

    // Helper Methods
    private static Dictionary<string, object> CreateCompleteValidPayload()
    {
        return new Dictionary<string, object>
        {
            { "Code", "PTN001" },
            { "Name", "Partner Express" },
            { "PersonType", "Natural Person" },
            { "ProfessionalTaxNumber", "PTX123456" },
            { "WithholdingTaxRate", "10.5" },
            { "HeadquartersCity", "Casablanca" },
            { "HeadquartersAddress", "123 Main Street" },
            { "LastName", "Doe" },
            { "FirstName", "John" },
            { "PhoneNumberContact", "+212612345678" },
            { "MailContact", "contact@partner.com" },
            { "FunctionContact", "Manager" },
            { "TransferType", "Bank Transfer" },
            { "AuthenticationMode", "SMS" },
            { "TaxIdentificationNumber", "TAX123456" },
            { "TaxRegime", "Standard" },
            { "AuxiliaryAccount", "AUX001" },
            { "ICE", "ICE123456789" },
            { "Logo", "/logos/partner001.png" },
            
            // All required IDs according to your validator
            { "PaymentModeId", Guid.NewGuid() },
            { "ActivityAccountId", Guid.NewGuid() },
            { "CommissionAccountId", Guid.NewGuid() },
            { "SupportAccountTypeId", Guid.NewGuid() },
            { "SupportAccountId", Guid.NewGuid() }
        };
    }

    private static Partner CreateTestPartner(string code, string name)
    {
        return Partner.Create(
            PartnerId.Of(Guid.NewGuid()),
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
        // Create a mock Bank first
        var mockBank = Bank.Create(
            BankId.Of(Guid.NewGuid()),
            "Mock Bank",
            "MOCK_BANK",
            "Mock Bank Description");

        // Create a mock ParamType for AccountType
        var mockAccountType = ParamType.Create(
            ParamTypeId.Of(Guid.NewGuid()),
            TypeDefinitionId.Of(Guid.NewGuid()),
            "Mock Account Type");

        return PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            "1234567890", // accountNumber
            "RIB123456789", // rib
            "Mock Domiciliation", // domiciliation
            "Mock Business Name", // businessName
            "Mock Short", // shortName
            1000.00m, // accountBalance
            mockBank, // bank
            mockAccountType); // accountType
    }

    private static SupportAccount CreateMockSupportAccount()
    {
        return SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            "SUPPORT001", // code
            "Mock Support Account", // description
            500.00m, // threshold
            10000.00m, // limit
            2000.00m, // accountBalance
            "ACC123456"); // accountingNumber
    }
}