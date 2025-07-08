using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractDetailsTests.PatchTests;

public class PatchContractDetailsEndpointTests : BaseAcceptanceTests
{
    public PatchContractDetailsEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {

        _contractRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractId id, CancellationToken _) => CreateMockContract(id.Value));

        _pricingRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PricingId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PricingId id, CancellationToken _) => CreateMockPricing(id.Value));
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var newPricingId = Guid.NewGuid();

        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(contractDetails);

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails>()); // No conflicts

        ContractDetails? updated = null;
        _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                                .Callback(() => updated = contractDetails)
                                .Returns(Task.CompletedTask);

        var payload = new
        {
            ContractDetailsId = id,
            PricingId = newPricingId,
            IsEnabled = false
            // ContractId intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.PricingId.Value.Should().Be(newPricingId);  // Should change
        updated.IsEnabled.Should().BeFalse();  // Should change
        updated.ContractId.Value.Should().Be(contractId); // Should not change

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} allows changing only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(contractDetails);

        ContractDetails? updated = null;
        _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                                .Callback(() => updated = contractDetails)
                                .Returns(Task.CompletedTask);

        var payload = new
        {
            ContractDetailsId = id,
            IsEnabled = false // Change from enabled to disabled
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.IsEnabled.Should().BeFalse(); // Should be disabled
        updated.ContractId.Value.Should().Be(contractId); // Should not change
        updated.PricingId.Value.Should().Be(pricingId); // Should not change

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} validates Contract exists when provided")]
    public async Task Patch_ShouldReturn404_WhenContractDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(contractDetails);

        var newContractId = Guid.NewGuid();
        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(newContractId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Contract?)null); // Not found

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = newContractId
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Contract with ID {newContractId} not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} validates Pricing exists when provided")]
    public async Task Patch_ShouldReturn404_WhenPricingDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(contractDetails);

        var newPricingId = Guid.NewGuid();
        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(newPricingId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Pricing?)null); // Not found

        var payload = new
        {
            ContractDetailsId = id,
            PricingId = newPricingId
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Pricing with ID {newPricingId} not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} returns 404 when contract details doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenContractDetailsDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync((ContractDetails?)null);

        var payload = new
        {
            ContractDetailsId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"ContractDetails [{id}] not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} returns 409 when new combination already exists")]
    public async Task Patch_ShouldReturn409_WhenNewCombinationAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var newContractId = Guid.NewGuid();

        var existing = CreateTestContractDetails(existingId, newContractId, pricingId);
        var target = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(target);

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails> { existing }); // Duplicate combination

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = newContractId  // This combination already exists for another contract details
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"ContractDetails with ContractId {newContractId} and PricingId {pricingId} already exists");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} returns 400 when empty ContractId provided")]
    public async Task Patch_ShouldReturn400_WhenEmptyContractIdProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = Guid.Empty // Empty ContractId should be invalid
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ContractId cannot be empty if provided");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contractdetails/{id} returns 400 when empty PricingId provided")]
    public async Task Patch_ShouldReturn400_WhenEmptyPricingIdProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            ContractDetailsId = id,
            PricingId = Guid.Empty // Empty PricingId should be invalid
        };

        // Act
        var response = await _client.PatchAsync($"/api/contractdetails/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("PricingId cannot be empty if provided");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper Methods
    private static ContractDetails CreateTestContractDetails(Guid id, Guid contractId, Guid pricingId)
    {
        return ContractDetails.Create(
            ContractDetailsId.Of(id),
            ContractId.Of(contractId),
            PricingId.Of(pricingId));
    }

    private static Contract CreateMockContract(Guid id)
    {
        return Contract.Create(
            ContractId.Of(id),
            "CTR001",
            PartnerId.Of(Guid.NewGuid()),
            DateTime.Today,
            DateTime.Today.AddDays(365));
    }

    private static Pricing CreateMockPricing(Guid id)
    {
        return Pricing.Create(
            PricingId.Of(id),
            "PRC001",
            "Online",
            100m,
            10000m,
            null,
            0.05m,
            CorridorId.Of(Guid.NewGuid()),
            ServiceId.Of(Guid.NewGuid()),
            null);
    }
}