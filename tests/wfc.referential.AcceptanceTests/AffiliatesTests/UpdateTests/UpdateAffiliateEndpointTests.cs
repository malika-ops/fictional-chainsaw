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
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AffiliatesTests.UpdateTests;

public class UpdateAffiliateEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAffiliateRepository> _repoMock = new();
    private readonly Mock<IParamTypeRepository> _paramTypeRepoMock = new();
    private readonly Mock<ICountryRepository> _countryRepoMock = new();

    public UpdateAffiliateEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAffiliateRepository>();
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<ICacheService>();

                // Setup default mocks that return success for valid scenarios
                SetupDefaultMocks();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_paramTypeRepoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private void SetupDefaultMocks()
    {
        // Setup repository save method
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup ParamType repository - return valid entities by default
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamTypeId id, CancellationToken _) => CreateMockParamType(id.Value));

        // Setup Country repository - return valid entities by default
        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountryId id, CancellationToken _) => CreateMockCountry(id.Value));
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 200 when update succeeds with all fields")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessfulWithAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldAffiliate = CreateTestAffiliate(id, "AFF001", "Old Affiliate");

        var countryId = Guid.NewGuid();
        var affiliateTypeId = Guid.NewGuid();

        // Reset mocks to ensure clean state
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();
        SetupDefaultMocks();

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldAffiliate);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Affiliate>()); // No conflicts

        // Setup Country mock for specific ID
        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(CreateMockCountry(countryId));

        // Setup ParamType mock for specific ID
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(affiliateTypeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(CreateMockParamType(affiliateTypeId));

        Affiliate updated = null;
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => updated = oldAffiliate)
                 .Returns(Task.CompletedTask);

        var payload = CreateCompleteUpdatePayload(id, countryId, affiliateTypeId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Read the response as boolean
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated.Code.Should().Be("AFF002");
        updated.Name.Should().Be("New Affiliate Name");
        updated.Abbreviation.Should().Be("NAF");
        updated.ThresholdBilling.Should().Be(15000.00m);
        updated.Logo.Should().Be("/logos/affiliate002.png");
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 400 when OpeningDate is missing")]
    public async Task Put_ShouldReturn400_WhenOpeningDateIsMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        var payload = new
        {
            AffiliateId = id,
            Code = "AFF001",
            Name = "Test Affiliate",
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
            // OpeningDate intentionally omitted
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OpeningDate is required");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 400 when AffiliateTypeId is missing")]
    public async Task Put_ShouldReturn400_WhenAffiliateTypeIdIsMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        var payload = new
        {
            AffiliateId = id,
            Code = "AFF001",
            Name = "Test Affiliate",
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            IsEnabled = true
            // AffiliateTypeId intentionally omitted
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AffiliateTypeId is required");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 400 when AccountingAccountNumber is missing")]
    public async Task Put_ShouldReturn400_WhenAccountingAccountNumberIsMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        var payload = new
        {
            AffiliateId = id,
            Code = "AFF001",
            Name = "Test Affiliate",
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
            // AccountingAccountNumber intentionally omitted
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AccountingAccountNumber is required");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} validates Country exists")]
    public async Task Put_ShouldReturn404_WhenCountryDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        // Reset mocks and setup for country not found scenario
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Affiliate>());

        var countryId = Guid.NewGuid();
        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Country?)null); // Not found

        var payload = CreateBasicUpdatePayload(id, countryId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Country with ID {countryId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} validates AffiliateType exists")]
    public async Task Put_ShouldReturn404_WhenAffiliateTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        // Reset mocks and setup for affiliate type not found scenario
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Affiliate>());

        // Setup successful country check
        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((CountryId id, CancellationToken _) => CreateMockCountry(id.Value));

        var affiliateTypeId = Guid.NewGuid();
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(affiliateTypeId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ParamType?)null); // Not found

        var payload = CreateBasicUpdatePayloadWithAffiliateType(id, affiliateTypeId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Affiliate Type with ID {affiliateTypeId} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            AffiliateId = id,
            // Code intentionally omitted
            Name = "New Affiliate Name",
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 409 when Code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetAffiliate = CreateTestAffiliate(id, "AFF001", "Target Affiliate");
        var conflictingAffiliate = CreateTestAffiliate(existingId, "AFF002", "Existing Affiliate");

        // Reset mocks and setup for conflict scenario
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(targetAffiliate);

        _repoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Affiliate> { conflictingAffiliate });

        // Setup other dependencies to succeed (shouldn't be reached)
        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((CountryId id, CancellationToken _) => CreateMockCountry(id.Value));

        var payload = CreateBasicUpdatePayloadWithCode(id, "AFF002");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} returns 404 when affiliate doesn't exist")]
    public async Task Put_ShouldReturn404_WhenAffiliateDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Reset mocks for affiliate not found scenario
        _repoMock.Reset();
        _paramTypeRepoMock.Reset();
        _countryRepoMock.Reset();

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Affiliate?)null);

        var payload = CreateBasicUpdatePayload(id, Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Affiliate [{id}] not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} validates field length limits")]
    public async Task Put_ShouldReturn400_WhenFieldsExceedLengthLimits()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        var payload = new
        {
            AffiliateId = id,
            Code = new string('X', 51), // Exceeds 50 char limit
            Name = new string('Y', 256), // Exceeds 255 char limit
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot exceed 50 characters");
        responseContent.Should().Contain("Name cannot exceed 255 characters");
    }

    [Fact(DisplayName = "PUT /api/affiliates/{id} validates ThresholdBilling is not negative")]
    public async Task Put_ShouldReturn400_WhenThresholdBillingIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _repoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(affiliate);

        var payload = new
        {
            AffiliateId = id,
            Code = "AFF001",
            Name = "Test Affiliate",
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = -100.00m, // Negative value
            AccountingAccountNumber = "411000001",
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/affiliates/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ThresholdBilling must be greater than or equal to 0");
    }

    // Helper methods
    private static object CreateCompleteUpdatePayload(Guid id, Guid countryId, Guid affiliateTypeId)
    {
        return new
        {
            AffiliateId = id,
            Code = "AFF002",
            Name = "New Affiliate Name",
            Abbreviation = "NAF",
            OpeningDate = DateTime.Now.AddDays(-60),
            CancellationDay = "15th of month",
            Logo = "/logos/affiliate002.png",
            ThresholdBilling = 15000.00m,
            AccountingDocumentNumber = "ACC-DOC-002",
            AccountingAccountNumber = "411000002",
            StampDutyMention = "No stamp duty",
            CountryId = countryId,
            IsEnabled = true,
            AffiliateTypeId = affiliateTypeId
        };
    }

    private static object CreateBasicUpdatePayload(Guid id, Guid countryId)
    {
        return new
        {
            AffiliateId = id,
            Code = "AFF001",
            Name = "Test Affiliate",
            OpeningDate = DateTime.Now,
            CountryId = countryId,
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
        };
    }

    private static object CreateBasicUpdatePayloadWithAffiliateType(Guid id, Guid affiliateTypeId)
    {
        return new
        {
            AffiliateId = id,
            Code = "AFF001",
            Name = "Test Affiliate",
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            IsEnabled = true,
            AffiliateTypeId = affiliateTypeId
        };
    }

    private static object CreateBasicUpdatePayloadWithCode(Guid id, string code)
    {
        return new
        {
            AffiliateId = id,
            Code = code,
            Name = "Test Affiliate",
            OpeningDate = DateTime.Now,
            CountryId = Guid.NewGuid(),
            ThresholdBilling = 10000.00m,
            AccountingAccountNumber = "411000001",
            AffiliateTypeId = Guid.NewGuid(),
            IsEnabled = true
        };
    }

    private static Affiliate CreateTestAffiliate(Guid id, string code, string name)
    {
        return Affiliate.Create(
            AffiliateId.Of(id),
            code,
            name,
            "WFC",
            DateTime.Now.AddDays(-30),
            "Last day of month",
            "/logos/affiliate.png",
            10000.00m,
            "ACC-DOC-001",
            "411000001",
            "Stamp duty applicable",
            CountryId.Of(Guid.NewGuid()));
    }

    // Mock entity creation methods - Updated to accept optional IDs
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