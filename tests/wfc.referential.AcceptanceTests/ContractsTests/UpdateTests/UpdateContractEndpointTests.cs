using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PartnerAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractsTests.UpdateTests;

public class UpdateContractEndpointTests : BaseAcceptanceTests
{
    public UpdateContractEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {
        _partnerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerId id, CancellationToken _) => CreateMockPartner(id.Value));
    }
  
    [Fact(DisplayName = "PUT /api/contracts/{id} returns 200 when update succeeds with all fields")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var oldContract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(oldContract);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract>()); // No conflicts

        Contract? updated = null;
        _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Callback(() => updated = oldContract)
                         .Returns(Task.CompletedTask);

        var payload = new
        {
            ContractId = id,
            Code = "CTR002",
            PartnerId = partnerId,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(400),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contracts/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated!.Code.Should().Be("CTR002");
        updated.StartDate.Should().Be(DateTime.Today.AddDays(5));
        updated.EndDate.Should().Be(DateTime.Today.AddDays(400));
        updated.IsEnabled.Should().BeTrue();

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/contracts/{id} returns 404 when contract doesn't exist")]
    public async Task Put_ShouldReturn404_WhenContractDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Contract?)null);

        var payload = new
        {
            ContractId = id,
            Code = "CTR001",
            PartnerId = Guid.NewGuid(),
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(365),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contracts/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Contract [{id}] not found");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/contracts/{id} returns 404 when partner doesn't exist")]
    public async Task Put_ShouldReturn404_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var contract = CreateTestContract(id, "CTR001", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(contract);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract>());

        var nonExistentPartnerId = Guid.NewGuid();
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(nonExistentPartnerId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Partner?)null);

        var payload = new
        {
            ContractId = id,
            Code = "CTR001",
            PartnerId = nonExistentPartnerId,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(365),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contracts/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner with ID {nonExistentPartnerId} not found");

        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/contracts/{id} returns 409 when Code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        var targetContract = CreateTestContract(id, "CTR001", partnerId);
        var conflictingContract = CreateTestContract(existingId, "CTR002", partnerId);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(targetContract);

        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<Contract> { conflictingContract });

        var payload = new
        {
            ContractId = id,
            Code = "CTR002", // This code already exists
            PartnerId = partnerId,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(365),
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contracts/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/contracts/{id} returns 400 when EndDate is before StartDate")]
    public async Task Put_ShouldReturn400_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        var payload = new
        {
            ContractId = id,
            Code = "CTR001",
            PartnerId = partnerId,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(5), // EndDate before StartDate
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/contracts/{id}", payload);

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