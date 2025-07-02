using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractDetailsTests.UpdateTests;

public class UpdateContractDetailsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IContractDetailsRepository> _contractDetailsRepoMock = new();
    private readonly Mock<IContractRepository> _contractRepoMock = new();
    private readonly Mock<IPricingRepository> _pricingRepoMock = new();

    public UpdateContractDetailsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IContractDetailsRepository>();
                services.RemoveAll<IContractRepository>();
                services.RemoveAll<IPricingRepository>();
                services.RemoveAll<ICacheService>();

                _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                _contractRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ContractId id, CancellationToken _) => CreateMockContract(id.Value));

                _pricingRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PricingId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PricingId id, CancellationToken _) => CreateMockPricing(id.Value));

                services.AddSingleton(_contractDetailsRepoMock.Object);
                services.AddSingleton(_contractRepoMock.Object);
                services.AddSingleton(_pricingRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "PUT /api/contractdetails/{id} returns 200 when update succeeds with all fields")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var newContractId = Guid.NewGuid();
        var newPricingId = Guid.NewGuid();

        var oldContractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(oldContractDetails);

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails>()); // No conflicts

        ContractDetails? updated = null;
        _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                                .Callback(() => updated = oldContractDetails)
                                .Returns(Task.CompletedTask);

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = newContractId,
            PricingId = newPricingId,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contractdetails/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.ContractId.Value.Should().Be(newContractId);
        updated.PricingId.Value.Should().Be(newPricingId);
        updated.IsEnabled.Should().BeFalse();

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/contractdetails/{id} returns 404 when contract details doesn't exist")]
    public async Task Put_ShouldReturn404_WhenContractDetailsDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync((ContractDetails?)null);

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = Guid.NewGuid(),
            PricingId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contractdetails/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"ContractDetails [{id}] not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/contractdetails/{id} returns 404 when contract doesn't exist")]
    public async Task Put_ShouldReturn404_WhenContractDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(contractDetails);

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails>());

        var nonExistentContractId = Guid.NewGuid();
        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(nonExistentContractId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Contract?)null);

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = nonExistentContractId,
            PricingId = pricingId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contractdetails/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Contract with ID {nonExistentContractId} not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/contractdetails/{id} returns 404 when pricing doesn't exist")]
    public async Task Put_ShouldReturn404_WhenPricingDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(id, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(contractDetails);

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails>());

        var nonExistentPricingId = Guid.NewGuid();
        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(nonExistentPricingId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Pricing?)null);

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = contractId,
            PricingId = nonExistentPricingId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contractdetails/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Pricing with ID {nonExistentPricingId} not found");

        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/contractdetails/{id} returns 409 when combination already exists")]
    public async Task Put_ShouldReturn409_WhenCombinationAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();

        var targetContractDetails = CreateTestContractDetails(id, Guid.NewGuid(), Guid.NewGuid());
        var conflictingContractDetails = CreateTestContractDetails(existingId, contractId, pricingId);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(targetContractDetails);

        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ContractDetails> { conflictingContractDetails });

        var payload = new
        {
            ContractDetailsId = id,
            ContractId = contractId,
            PricingId = pricingId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contractdetails/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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