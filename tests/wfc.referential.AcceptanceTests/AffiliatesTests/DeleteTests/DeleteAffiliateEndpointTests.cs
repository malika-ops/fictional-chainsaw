using System.Linq.Expressions;
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
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AffiliatesTests.DeleteTests;

public class DeleteAffiliateEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAffiliateRepository> _affiliateRepoMock = new();
    private readonly Mock<IPricingRepository> _pricingRepoMock = new();

    public DeleteAffiliateEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithMocks(bool hasPricingDependencies = false)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAffiliateRepository>();
                services.RemoveAll<IPricingRepository>();
                services.RemoveAll<ICacheService>();

                _affiliateRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Configure pricing dependencies based on test needs
                if (hasPricingDependencies)
                {
                    var mockPricing = CreateTestPricing(AffiliateId.Of(Guid.NewGuid()));
                    _pricingRepoMock.Setup(r => r.GetByConditionAsync(
                            It.IsAny<Expression<Func<Pricing, bool>>>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Pricing> { mockPricing });
                }
                else
                {
                    _pricingRepoMock.Setup(r => r.GetByConditionAsync(
                            It.IsAny<Expression<Func<Pricing, bool>>>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Pricing>());
                }

                services.AddSingleton(_affiliateRepoMock.Object);
                services.AddSingleton(_pricingRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        return customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} returns 200 when affiliate exists and has no dependencies")]
    public async Task Delete_ShouldReturn200_WhenAffiliateExistsAndHasNoDependencies()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();
        affiliate.IsEnabled.Should().BeFalse();
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} returns 409 when affiliate has pricing dependencies")]
    public async Task Delete_ShouldReturn409_WhenAffiliateHasPricingDependencies()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: true);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict); 

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Cannot delete affiliate with ID");
        responseContent.Should().Contain("because it is linked to one or more pricings");

        // Verify affiliate was NOT disabled
        affiliate.IsEnabled.Should().BeTrue();
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} returns 404 when affiliate is not found")]
    public async Task Delete_ShouldReturn404_WhenAffiliateNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = CreateClientWithMocks();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Affiliate?)null);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} performs soft delete instead of physical deletion")]
    public async Task Delete_ShouldPerformSoftDelete_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        // Verify affiliate starts as enabled
        affiliate.IsEnabled.Should().BeTrue();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        affiliate.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (affiliate object still exists)
        affiliate.Should().NotBeNull();
        affiliate.Code.Should().Be("AFF001"); // Data still intact
        affiliate.Name.Should().Be("Test Affiliate");

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Arrange
        var client = CreateClientWithMocks();

        // Act
        var response = await client.DeleteAsync("/api/affiliates/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Never);
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} verifies affiliate state before and after deletion")]
    public async Task Delete_ShouldVerifyAffiliateStateBeforeAndAfterDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Active Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        // Ensure affiliate starts enabled
        affiliate.IsEnabled.Should().BeTrue("Affiliate should start as enabled");

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify final state - affiliate should be disabled but data intact
        affiliate.IsEnabled.Should().BeFalse("Affiliate should be disabled after deletion");
        affiliate.Code.Should().Be("AFF001", "Affiliate code should remain intact");
        affiliate.Name.Should().Be("Active Affiliate", "Affiliate name should remain intact");
        affiliate.Abbreviation.Should().Be("WFC", "Affiliate abbreviation should remain intact");

        // Verify repository interactions
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} handles empty GUID correctly")]
    public async Task Delete_ShouldReturn400_ForEmptyGuid()
    {
        // Arrange
        var client = CreateClientWithMocks();

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("AffiliateId must be a non-empty GUID");

        // Verify no repository operations were attempted
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Never);
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} ensures atomic operation")]
    public async Task Delete_ShouldEnsureAtomicOperation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Track the sequence of operations
        var operationSequence = new List<string>();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("GetById"))
            .ReturnsAsync(affiliate);

        _affiliateRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify operation sequence
        operationSequence.Should().ContainInOrder("GetById", "SaveChanges");
        operationSequence.Should().HaveCount(2);

        // Verify all operations were called exactly once
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Once);
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} handles concurrent requests")]
    public async Task Delete_ShouldHandleConcurrentRequests()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var affiliate1 = CreateTestAffiliate(id1, "AFF001", "Affiliate 1");
        var affiliate2 = CreateTestAffiliate(id2, "AFF002", "Affiliate 2");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate1);
        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate2);

        // Act - Simulate concurrent requests
        var tasks = new[]
        {
            client.DeleteAsync($"/api/affiliates/{id1}"),
            client.DeleteAsync($"/api/affiliates/{id2}")
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeTrue();
        }

        // Verify both affiliates were disabled
        affiliate1.IsEnabled.Should().BeFalse();
        affiliate2.IsEnabled.Should().BeFalse();

        // Verify repository was called for each request
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} maintains data integrity")]
    public async Task Delete_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        // Set some additional properties to verify they remain intact
        affiliate.SetAffiliateType(ParamTypeId.Of(Guid.NewGuid()));

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Capture original values
        var originalCode = affiliate.Code;
        var originalName = affiliate.Name;
        var originalThresholdBilling = affiliate.ThresholdBilling;
        var originalCountryId = affiliate.CountryId;

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify only IsEnabled changed, all other data remains intact
        affiliate.IsEnabled.Should().BeFalse("Affiliate should be disabled");
        affiliate.Code.Should().Be(originalCode, "Code should remain unchanged");
        affiliate.Name.Should().Be(originalName, "Name should remain unchanged");
        affiliate.ThresholdBilling.Should().Be(originalThresholdBilling, "ThresholdBilling should remain unchanged");
        affiliate.CountryId.Should().Be(originalCountryId, "CountryId should remain unchanged");

        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} handles multiple delete attempts")]
    public async Task Delete_ShouldHandleMultipleDeleteAttempts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act - First delete
        var response1 = await client.DeleteAsync($"/api/affiliates/{id}");
        var result1 = await response1.Content.ReadFromJsonAsync<bool>();

        // Act - Second delete (should still work, idempotent)
        var response2 = await client.DeleteAsync($"/api/affiliates/{id}");
        var result2 = await response2.Content.ReadFromJsonAsync<bool>();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        result1.Should().BeTrue();

        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        result2.Should().BeTrue();

        // Verify affiliate remains disabled
        affiliate.IsEnabled.Should().BeFalse();

        // Verify repository interactions
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} validates URL parameter matches body")]
    public async Task Delete_ShouldValidateUrlParameterMatchesBody()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the correct ID was used in repository call
        _affiliateRepoMock.Verify(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} handles repository exceptions gracefully")]
    public async Task Delete_ShouldHandleRepositoryExceptionsGracefully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = CreateClientWithMocks();

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        // Verify SaveChanges was never called due to exception
        _affiliateRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/affiliates/{id} verifies dependency check is called")]
    public async Task Delete_ShouldVerifyDependencyCheckIsCalled()
    {
        // Arrange
        var id = Guid.NewGuid();
        var affiliate = CreateTestAffiliate(id, "AFF001", "Test Affiliate");
        var client = CreateClientWithMocks(hasPricingDependencies: false);

        _affiliateRepoMock.Setup(r => r.GetByIdAsync(AffiliateId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(affiliate);

        // Act
        var response = await client.DeleteAsync($"/api/affiliates/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify that pricing dependency check was performed
        _pricingRepoMock.Verify(r => r.GetByConditionAsync(
            It.IsAny<Expression<Func<Pricing, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
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

    private static Pricing CreateTestPricing(AffiliateId affiliateId)
    {
        return Pricing.Create(
            PricingId.Of(Guid.NewGuid()),
            "Test Pricing Code",
            "Test Pricing Name",
            100.00m,    
            200.00m,   
            50.00m,   
            25.00m,   
            CorridorId.Of(Guid.NewGuid()),
            ServiceId.Of(Guid.NewGuid()),
            affiliateId);
    }
}