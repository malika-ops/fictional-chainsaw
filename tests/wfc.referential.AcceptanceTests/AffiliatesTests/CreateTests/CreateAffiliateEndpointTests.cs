using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.AcceptanceTests.AffiliatesTests.CreateTests;

public class CreateAffiliateEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAffiliateRepository> _repoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();
    private readonly Mock<ICountryRepository> _countryRepoMock = new();

    public CreateAffiliateEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove all the services we want to mock
                services.RemoveAll<IAffiliateRepository>();
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<ICacheService>();

                // Setup default mocks that return success for valid scenarios
                SetupDefaultMocks();

                // Register mocked services
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_paramTypeRepoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    private void SetupDefaultMocks()
    {
        // Setup Affiliate Repository - default behavior for successful scenarios
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Affiliate a, CancellationToken _) => a);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        // Setup ParamType repository - return valid entities by default
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType(id.Value));

        // Setup Country repository - return valid entities by default
        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountryId id, CancellationToken _) => CreateMockCountry(id.Value));
    }

    [Fact(DisplayName = "POST /api/affiliates returns 200 and Guid when all required fields are provided")]
    public async Task Post_ShouldReturn200_AndId_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        Affiliate capturedCreateAffiliate = null;

        // Reset mocks to ensure clean state
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();
        SetupDefaultMocks();

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()))
            .Callback<Affiliate, CancellationToken>((a, _) => capturedCreateAffiliate = a)
            .ReturnsAsync((Affiliate a, CancellationToken _) => a);

        var payload = CreateCompleteValidPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedCreateAffiliate.Should().NotBeNull();
        capturedCreateAffiliate.Code.Should().Be((string)payload["Code"]);
        capturedCreateAffiliate.Name.Should().Be((string)payload["Name"]);
        capturedCreateAffiliate.Abbreviation.Should().Be((string)payload["Abbreviation"]);
        capturedCreateAffiliate.IsEnabled.Should().BeTrue(); // IsEnabled = true by default
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("Code");

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when Name is missing")]
    public async Task Post_ShouldReturn400_WhenNameIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("Name");

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Name is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when OpeningDate is missing")]
    public async Task Post_ShouldReturn400_WhenOpeningDateIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("OpeningDate");

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OpeningDate is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when AffiliateTypeId is missing")]
    public async Task Post_ShouldReturn400_WhenAffiliateTypeIdIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("AffiliateTypeId");

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AffiliateTypeId is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when AccountingAccountNumber is missing")]
    public async Task Post_ShouldReturn400_WhenAccountingAccountNumberIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("AccountingAccountNumber");

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AccountingAccountNumber is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when CountryId is missing")]
    public async Task Post_ShouldReturn400_WhenCountryIdIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("CountryId");

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("CountryId must be a valid GUID");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        const string duplicateCode = "AFF001";

        // Reset mocks and setup for conflict scenario
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();

        var existingAffiliate = CreateTestAffiliate(duplicateCode, "Existing Affiliate");

        // Setup to return existing affiliate with same code
        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate> { existingAffiliate });

        // Setup other dependencies to succeed (shouldn't be reached)
        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountryId id, CancellationToken _) => CreateMockCountry(id.Value));

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType(id.Value));

        var payload = CreateCompleteValidPayload();
        payload["Code"] = duplicateCode;

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", payload);

        // Assert - Your GlobalExceptionHandler correctly returns 409 for ConflictException
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Affiliate with code {duplicateCode} already exists");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 404 when Country does not exist")]
    public async Task Post_ShouldReturn404_WhenCountryDoesNotExist()
    {
        // Arrange
        var invalidCountryId = Guid.NewGuid();

        // Reset mocks and setup for country not found scenario
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();

        // Setup successful affiliate code check (no duplicates)
        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        // Setup successful param type check
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType(id.Value));

        // Setup Country to return null for the specific invalid ID
        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(invalidCountryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var payload = CreateCompleteValidPayload();
        payload["CountryId"] = invalidCountryId;

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", payload);

        // Assert - Your GlobalExceptionHandler correctly returns 404 for ResourceNotFoundException
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Country with ID {invalidCountryId} not found");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 404 when AffiliateType does not exist")]
    public async Task Post_ShouldReturn404_WhenAffiliateTypeDoesNotExist()
    {
        // Arrange
        var invalidAffiliateTypeId = Guid.NewGuid();

        // Reset and setup specific mock behavior for this test
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();
        _repoMock.Reset();

        // Setup successful country lookup
        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountryId id, CancellationToken _) => CreateMockCountry(id.Value));

        // Setup successful affiliate code check (no duplicates)
        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        // Setup ParamType to return null for the specific invalid ID
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(invalidAffiliateTypeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        var payload = CreateCompleteValidPayload();
        payload["AffiliateTypeId"] = invalidAffiliateTypeId;

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", payload);

        // Assert - Your GlobalExceptionHandler correctly returns 404 for ResourceNotFoundException
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Affiliate Type with ID {invalidAffiliateTypeId} not found");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates validates ThresholdBilling is not negative")]
    public async Task Post_ShouldReturn400_WhenThresholdBillingIsNegative()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["ThresholdBilling"] = -100.0m;

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ThresholdBilling must be greater than or equal to 0");
    }

    [Fact(DisplayName = "POST /api/affiliates validates field length limits")]
    public async Task Post_ShouldReturn400_WhenFieldsExceedLengthLimits()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["Code"] = new string('X', 51); // Exceeds 50 char limit
        invalidPayload["Name"] = new string('Y', 256); // Exceeds 255 char limit
        invalidPayload["Abbreviation"] = new string('Z', 11); // Exceeds 10 char limit

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot exceed 50 characters");
        responseContent.Should().Contain("Name cannot exceed 255 characters");
        responseContent.Should().Contain("Abbreviation cannot exceed 10 characters");
    }

    [Fact(DisplayName = "POST /api/affiliates allows creation with minimal required fields")]
    public async Task Post_ShouldReturn200_WithMinimalRequiredFields()
    {
        // Arrange
        Affiliate capturedCreateAffiliate = null;

        // Reset mocks to ensure clean state
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();
        SetupDefaultMocks();

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()))
            .Callback<Affiliate, CancellationToken>((a, _) => capturedCreateAffiliate = a)
            .ReturnsAsync((Affiliate a, CancellationToken _) => a);

        var minimalPayload = CreateMinimalValidPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", minimalPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().NotBeEmpty();

        capturedCreateAffiliate.Should().NotBeNull();
        capturedCreateAffiliate.Code.Should().Be((string)minimalPayload["Code"]);
        capturedCreateAffiliate.Name.Should().Be((string)minimalPayload["Name"]);
        capturedCreateAffiliate.ThresholdBilling.Should().Be(0); // Default value
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when CountryId is empty")]
    public async Task Post_ShouldReturn400_WhenCountryIdIsEmpty()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["CountryId"] = Guid.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("CountryId must be a valid GUID");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when AffiliateTypeId is empty")]
    public async Task Post_ShouldReturn400_WhenAffiliateTypeIdIsEmpty()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["AffiliateTypeId"] = Guid.Empty;

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AffiliateTypeId is required and must be a valid GUID");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when Code is empty")]
    public async Task Post_ShouldReturn400_WhenCodeIsEmpty()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["Code"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when Name is empty")]
    public async Task Post_ShouldReturn400_WhenNameIsEmpty()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["Name"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Name is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/affiliates returns 400 when AccountingAccountNumber is empty")]
    public async Task Post_ShouldReturn400_WhenAccountingAccountNumberIsEmpty()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["AccountingAccountNumber"] = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/affiliates", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AccountingAccountNumber is required");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Affiliate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper Methods
    private static Dictionary<string, object> CreateCompleteValidPayload()
    {
        return new Dictionary<string, object>
        {
            { "Code", "AFF001" },
            { "Name", "Wafacash" },
            { "Abbreviation", "WFC" },
            { "OpeningDate", DateTime.Now.AddDays(-30) },
            { "CancellationDay", "Last day of month" },
            { "Logo", "/logos/affiliate001.png" },
            { "ThresholdBilling", 10000.00m },
            { "AccountingDocumentNumber", "ACC-DOC-001" },
            { "AccountingAccountNumber", "411000001" },
            { "StampDutyMention", "Stamp duty applicable" },
            { "CountryId", Guid.NewGuid() },
            { "AffiliateTypeId", Guid.NewGuid() }
        };
    }

    private static Dictionary<string, object> CreateMinimalValidPayload()
    {
        return new Dictionary<string, object>
        {
            { "Code", "AFF001" },
            { "Name", "Wafacash" },
            { "OpeningDate", DateTime.Now.AddDays(-30) },
            { "AccountingAccountNumber", "411000001" },
            { "CountryId", Guid.NewGuid() },
            { "AffiliateTypeId", Guid.NewGuid() }
        };
    }

    private static Affiliate CreateTestAffiliate(string code, string name)
    {
        return Affiliate.Create(
            AffiliateId.Of(Guid.NewGuid()),
            code,
            name,
            "WFC", // abbreviation
            DateTime.Now.AddDays(-30), // openingDate
            "Last day of month", // cancellationDay
            "/logos/affiliate.png", // logo
            10000.00m, // thresholdBilling
            "ACC-DOC-001", // accountingDocumentNumber
            "411000001", // accountingAccountNumber
            "Stamp duty applicable", // stampDutyMention
            CountryId.Of(Guid.NewGuid())); // countryId
    }

    // Mock entity creation methods
    private static ParamType CreateMockParamType(Guid? id = null)
    {
        return ParamType.Create(
            ParamTypeId.Of(id ?? Guid.NewGuid()),
            TypeDefinitionId.Of(Guid.NewGuid()),
            "Mock ParamType Value");
    }

    private static Country CreateMockCountry(Guid? id = null)
    {
        return Country.Create(
            CountryId.Of(id ?? Guid.NewGuid()),
            "MA",
            "Morocco",
            "MAR",
            "MA",
            "MAR",
            "+212",
            "GMT+1",
            true,
            true,
            2,
            MonetaryZoneId.Of(Guid.NewGuid()),
            CurrencyId.Of(Guid.NewGuid()));
    }
}