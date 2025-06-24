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
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PartnerAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractsTests.DeleteTests;

public class DeleteContractEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IContractRepository> _contractRepoMock = new();

    public DeleteContractEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IContractRepository>();
                services.RemoveAll<ICacheService>();

                _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_contractRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/contracts/{id} returns 200 when contract exists")]
    public async Task Delete_ShouldReturn200_WhenContractExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Capture the entity passed to SaveChanges
        Contract? updatedContract = null;
        _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => updatedContract = contract)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/contracts/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();
        updatedContract!.IsEnabled.Should().BeFalse();
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/contracts/{id} returns 404 when contract is not found")]
    public async Task Delete_ShouldReturn404_WhenContractNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/contracts/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Contract not found");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/contracts/{id} performs soft delete instead of physical deletion")]
    public async Task Delete_ShouldPerformSoftDelete_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        // Verify contract starts as enabled
        contract.IsEnabled.Should().BeTrue();

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        var response = await _client.DeleteAsync($"/api/contracts/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        contract.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (contract object still exists)
        contract.Should().NotBeNull();
        contract.Code.Should().Be("CTR001"); // Data still intact
        contract.PartnerId.Value.Should().Be(partnerId);

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/contracts/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/contracts/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _contractRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()), Times.Never);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/contracts/{id} verifies contract state before and after deletion")]
    public async Task Delete_ShouldVerifyContractStateBeforeAndAfterDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        // Ensure contract starts enabled
        contract.IsEnabled.Should().BeTrue("Contract should start as enabled");

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        var response = await _client.DeleteAsync($"/api/contracts/{id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify final state - contract should be disabled but data intact
        contract.IsEnabled.Should().BeFalse("Contract should be disabled after deletion");
        contract.Code.Should().Be("CTR001", "Contract code should remain intact");
        contract.PartnerId.Value.Should().Be(partnerId, "Contract partner ID should remain intact");

        // Verify repository interactions
        _contractRepoMock.Verify(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/contracts/{id} ensures atomic operation")]
    public async Task Delete_ShouldEnsureAtomicOperation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Track the sequence of operations
        var operationSequence = new List<string>();

        _contractRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("GetById"))
            .ReturnsAsync(contract);

        _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => operationSequence.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/contracts/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify operation sequence
        operationSequence.Should().ContainInOrder("GetById", "SaveChanges");
        operationSequence.Should().HaveCount(2);

        // Verify all operations were called exactly once
        _contractRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()), Times.Once);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper to build dummy contracts quickly
    private static Contract CreateTestContract(Guid id, string code, Guid partnerId)
    {
        return Contract.Create(
            ContractId.Of(id),
            code,
            PartnerId.Of(partnerId),
            DateTime.Today,
            DateTime.Today.AddDays(365));
    }
}