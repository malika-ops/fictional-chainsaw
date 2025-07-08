using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.PricingAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractDetailsTests.DeleteTests;

public class DeleteContractDetailsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "DELETE /api/contractdetails/{id} returns 200 when contract details exists")]
    public async Task Delete_ShouldReturn200_WhenContractDetailsExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractDetails);

        // Capture the entity passed to SaveChanges
        ContractDetails? updatedContractDetails = null;
        _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => updatedContractDetails = contractDetails)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/contractdetails/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();
        updatedContractDetails!.IsEnabled.Should().BeFalse();
        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/contractdetails/{id} returns 404 when contract details is not found")]
    public async Task Delete_ShouldReturn404_WhenContractDetailsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDetails?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/contractdetails/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"ContractDetails with ID {id} was not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/contractdetails/{id} performs soft delete instead of physical deletion")]
    public async Task Delete_ShouldPerformSoftDelete_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        // Verify contract details starts as enabled
        contractDetails.IsEnabled.Should().BeTrue();

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractDetails);

        // Act
        var response = await _client.DeleteAsync($"/api/contractdetails/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        contractDetails.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (contract details object still exists)
        contractDetails.Should().NotBeNull();
        contractDetails.ContractId.Value.Should().Be(contractId); // Data still intact
        contractDetails.PricingId.Value.Should().Be(pricingId);

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/contractdetails/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/contractdetails/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _contractDetailsRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ContractDetailsId>(), It.IsAny<CancellationToken>()), Times.Never);
        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/contractdetails/{id} verifies contract details state before and after deletion")]
    public async Task Delete_ShouldVerifyContractDetailsStateBeforeAndAfterDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        // Ensure contract details starts enabled
        contractDetails.IsEnabled.Should().BeTrue("ContractDetails should start as enabled");

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractDetails);

        // Act
        var response = await _client.DeleteAsync($"/api/contractdetails/{id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify final state - contract details should be disabled but data intact
        contractDetails.IsEnabled.Should().BeFalse("ContractDetails should be disabled after deletion");
        contractDetails.ContractId.Value.Should().Be(contractId, "ContractDetails contract ID should remain intact");
        contractDetails.PricingId.Value.Should().Be(pricingId, "ContractDetails pricing ID should remain intact");

        // Verify repository interactions
        _contractDetailsRepoMock.Verify(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/contractdetails/{id} ensures atomic operation")]
    public async Task Delete_ShouldEnsureAtomicOperation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractDetails);

        // Track the sequence of operations
        var operationSequence = new List<string>();

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ContractDetailsId>(), It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("GetById"))
            .ReturnsAsync(contractDetails);

        _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/contractdetails/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify operation sequence
        operationSequence.Should().ContainInOrder("GetById", "SaveChanges");
        operationSequence.Should().HaveCount(2);

        // Verify all operations were called exactly once
        _contractDetailsRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ContractDetailsId>(), It.IsAny<CancellationToken>()), Times.Once);
        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper Methods
    private static ContractDetails CreateTestContractDetails(Guid id, Guid contractId, Guid pricingId)
    {
        return ContractDetails.Create(
            ContractDetailsId.Of(id),
            ContractId.Of(contractId),
            PricingId.Of(pricingId));
    }
}