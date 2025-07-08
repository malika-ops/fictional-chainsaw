using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AffiliatesTests.PatchTests;

public class PatchAffiliateEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PATCH /api/affiliates/{id} returns 200 when patching single field")]
    public async Task Patch_ShouldReturn200_WhenPatchingSingleField()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Old Name");
        var originalThresholdBilling = affiliate.ThresholdBilling;

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        _affiliateRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        var patchPayload = new { AffiliateId = id, Name = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify only the Name was updated, other fields remain unchanged
        affiliate.Name.Should().Be("Updated Name");
        affiliate.Code.Should().Be("AFF001"); // Should remain unchanged
        affiliate.ThresholdBilling.Should().Be(originalThresholdBilling); // Should remain unchanged

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} returns 200 when patching multiple fields")]
    public async Task Patch_ShouldReturn200_WhenPatchingMultipleFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Old Name");
        var originalLogo = affiliate.Logo;

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        _affiliateRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        var patchPayload = new
        {
            AffiliateId = id,
            Name = "Updated Name",
            ThresholdBilling = 20000.00m,
            IsEnabled = false
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify specified fields were updated
        affiliate.Name.Should().Be("Updated Name");
        affiliate.ThresholdBilling.Should().Be(20000.00m);
        affiliate.IsEnabled.Should().BeFalse();

        // Verify unspecified fields remained unchanged
        affiliate.Code.Should().Be("AFF001");
        affiliate.Logo.Should().Be(originalLogo);

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} returns 404 when affiliate not found")]
    public async Task Patch_ShouldReturn404_WhenAffiliateNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Affiliate?)null);

        var patchPayload = new { AffiliateId = id, Name = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Affiliate [{id}] not found");

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} returns 409 when code already exists")]
    public async Task Patch_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetAffiliate = CreateTestAffiliate(id, "AFF001", "Target Affiliate");
        var conflictingAffiliate = CreateTestAffiliate(existingId, "AFF002", "Existing Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetAffiliate);

        _affiliateRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate> { conflictingAffiliate });

        var patchPayload = new { AffiliateId = id, Code = "AFF002" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AFF002 already exists");

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} validates Country exists when provided")]
    public async Task Patch_ShouldReturn404_WhenCountryDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var invalidCountryId = Guid.NewGuid();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(invalidCountryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var patchPayload = new { AffiliateId = id, CountryId = invalidCountryId };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Country with ID {invalidCountryId} not found");

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} validates AffiliateType exists when provided")]
    public async Task Patch_ShouldReturn404_WhenAffiliateTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var invalidAffiliateTypeId = Guid.NewGuid();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(invalidAffiliateTypeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        var patchPayload = new { AffiliateId = id, AffiliateTypeId = invalidAffiliateTypeId };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Affiliate Type with ID {invalidAffiliateTypeId} not found");

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} returns 400 for empty GUID")]
    public async Task Patch_ShouldReturn400_ForEmptyGuid()
    {
        // Arrange
        var patchPayload = new { AffiliateId = Guid.Empty, Name = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{Guid.Empty}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AffiliateId cannot be empty");

        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} validates field length limits")]
    public async Task Patch_ShouldReturn400_WhenFieldsExceedLengthLimits()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        var patchPayload = new
        {
            AffiliateId = id,
            Code = new string('X', 51), // Exceeds 50 char limit
            Name = new string('Y', 256), // Exceeds 255 char limit
            Abbreviation = new string('Z', 11) // Exceeds 10 char limit
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot exceed 50 characters");
        responseContent.Should().Contain("Name cannot exceed 255 characters");
        responseContent.Should().Contain("Abbreviation cannot exceed 10 characters");
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} validates ThresholdBilling is not negative")]
    public async Task Patch_ShouldReturn400_WhenThresholdBillingIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        var patchPayload = new
        {
            AffiliateId = id,
            ThresholdBilling = -100.00m
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ThresholdBilling must be greater than or equal to 0");
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} allows empty patch (no fields to update)")]
    public async Task Patch_ShouldReturn200_WhenNoFieldsToUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        var patchPayload = new { AffiliateId = id }; // Only ID, no fields to update

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} handles null values correctly")]
    public async Task Patch_ShouldReturn400_WhenProvidingEmptyStrings()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        var patchPayload = new
        {
            AffiliateId = id,
            Code = "", // Empty string should fail validation
            Name = ""  // Empty string should fail validation
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot be empty if provided");
        responseContent.Should().Contain("Name cannot be empty if provided");
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} allows partial update with valid Country and AffiliateType")]
    public async Task Patch_ShouldReturn200_WhenUpdatingCountryAndAffiliateType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var affiliateTypeId = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        _affiliateRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        // Setup Country mock for specific ID
        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockCountry());

        // Setup ParamType mock for specific ID
        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(ParamTypeId.Of(affiliateTypeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockParamType());

        var patchPayload = new
        {
            AffiliateId = id,
            CountryId = countryId,
            AffiliateTypeId = affiliateTypeId
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify repository interactions
        _countryRepoMock.Verify(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()), Times.Once);
        _paramTypeRepoMock.Verify(r => r.GetByIdAsync(ParamTypeId.Of(affiliateTypeId), It.IsAny<CancellationToken>()), Times.Once);
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} handles concurrent patch requests")]
    public async Task Patch_ShouldHandleConcurrentPatchRequests()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var affiliate1 = CreateTestAffiliate(id1, "AFF001", "Affiliate 1");
        var affiliate2 = CreateTestAffiliate(id2, "AFF002", "Affiliate 2");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate1);
        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate2);

        _affiliateRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Affiliate, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Affiliate>());

        var patchPayload1 = new { AffiliateId = id1, Name = "Updated Affiliate 1" };
        var patchPayload2 = new { AffiliateId = id2, Name = "Updated Affiliate 2" };

        // Act - Simulate concurrent requests
        var tasks = new[]
        {
            _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id1}")
            {
                Content = JsonContent.Create(patchPayload1)
            }),
            _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{id2}")
            {
                Content = JsonContent.Create(patchPayload2)
            })
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeTrue();
        }

        // Verify both affiliates were updated
        affiliate1.Name.Should().Be("Updated Affiliate 1");
        affiliate2.Name.Should().Be("Updated Affiliate 2");

        // Verify repository was called for each request
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "PATCH /api/affiliates/{id} validates URL parameter matches request body")]
    public async Task Patch_ShouldValidateUrlParameterMatchesRequestBody()
    {
        // Arrange
        var urlId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(urlId, "AFF001", "Test Affiliate");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(urlId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        var patchPayload = new { AffiliateId = bodyId, Name = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/affiliates/{urlId}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the URL ID was used (not the body ID)
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(AffiliateId.Of(urlId), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper methods
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

    private static ParamType CreateMockParamType()
    {
        return ParamType.Create(
            ParamTypeId.Of(Guid.NewGuid()),
            TypeDefinitionId.Of(Guid.NewGuid()),
            "Mock ParamType Value");
    }

    private static Country CreateMockCountry()
    {
        return Country.Create(
            CountryId.Of(Guid.NewGuid()),
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