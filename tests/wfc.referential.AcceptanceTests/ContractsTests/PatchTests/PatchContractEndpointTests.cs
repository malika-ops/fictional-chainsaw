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

namespace wfc.referential.AcceptanceTests.ContractsTests.PatchTests;

public class PatchContractEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IContractRepository> _contractRepoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();

    public PatchContractEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IContractRepository>();
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ICacheService>();

                _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                _partnerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerId id, CancellationToken _) => CreateMockPartner(id.Value));

                services.AddSingleton(_contractRepoMock.Object);
                services.AddSingleton(_partnerRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(contract);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract>()); // No conflicts

        Contract? updated = null;
        _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Callback(() => updated = contract)
                         .Returns(Task.CompletedTask);

        var payload = new
        {
            ContractId = id,
            Code = "CTR002",
            EndDate = DateTime.Today.AddDays(500)
            // Other fields intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.Code.Should().Be("CTR002");  // Should change
        updated.EndDate.Should().Be(DateTime.Today.AddDays(500));  // Should change
        updated.PartnerId.Value.Should().Be(partnerId); // Should not change
        updated.StartDate.Should().Be(DateTime.Today); // Should not change
        updated.IsEnabled.Should().BeTrue(); // Should remain enabled

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} allows changing only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(contract);

        Contract? updated = null;
        _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Callback(() => updated = contract)
                         .Returns(Task.CompletedTask);

        var payload = new
        {
            ContractId = id,
            IsEnabled = false // Change from enabled to disabled
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.IsEnabled.Should().BeFalse(); // Should be disabled
        updated.Code.Should().Be("CTR001"); // Should not change
        updated.PartnerId.Value.Should().Be(partnerId); // Should not change

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} validates Partner exists when provided")]
    public async Task Patch_ShouldReturn404_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(contract);

        var newPartnerId = Guid.NewGuid();
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(newPartnerId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Partner?)null); // Not found

        var payload = new
        {
            ContractId = id,
            PartnerId = newPartnerId
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner with ID {newPartnerId} not found");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} returns 404 when contract doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenContractDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Contract?)null);

        var payload = new
        {
            ContractId = id,
            Code = "CTR002"
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Contract [{id}] not found");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} returns 409 when new code already exists")]
    public async Task Patch_ShouldReturn409_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        var existing = CreateTestContract(existingId, "CTR002", partnerId);
        var target = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(target);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { existing }); // Duplicate code

        var payload = new
        {
            ContractId = id,
            Code = "CTR002"  // This code already exists for another contract
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Contract with code CTR002 already exists");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} returns 400 when empty code provided")]
    public async Task Patch_ShouldReturn400_WhenEmptyCodeProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            ContractId = id,
            Code = "" // Empty code should be invalid
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot be empty if provided");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/contracts/{id} validates date range when both dates provided")]
    public async Task Patch_ShouldReturn400_WhenEndDateBeforeStartDate()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            ContractId = id,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(5) // EndDate before StartDate
        };

        // Act
        var response = await _client.PatchAsync($"/api/contracts/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("EndDate must be greater than StartDate");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper Methods
    private static Contract CreateTestContract(Guid id, string code, Guid partnerId)
    {
        return Contract.Create(
            ContractId.Of(id),
            code,
            PartnerId.Of(partnerId),
            DateTime.Today,
            DateTime.Today.AddDays(365));
    }

    private static Partner CreateMockPartner(Guid id)
    {
        return Partner.Create(
            PartnerId.Of(id),
            "PTN001",
            "Test Partner",
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
            "/logos/logo.png");
    }
}