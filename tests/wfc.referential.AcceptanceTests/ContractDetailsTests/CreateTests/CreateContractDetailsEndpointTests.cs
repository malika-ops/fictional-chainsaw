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

namespace wfc.referential.AcceptanceTests.ContractDetailsTests.CreateTests;

public class CreateContractDetailsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IContractDetailsRepository> _contractDetailsRepoMock = new();
    private readonly Mock<IContractRepository> _contractRepoMock = new();
    private readonly Mock<IPricingRepository> _pricingRepoMock = new();

    public CreateContractDetailsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IContractDetailsRepository>();
                services.RemoveAll<IContractRepository>();
                services.RemoveAll<IPricingRepository>();
                services.RemoveAll<ICacheService>();

                // Setup ContractDetails Repository
                _contractDetailsRepoMock.Setup(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ContractDetails cd, CancellationToken _) => cd);
                _contractDetailsRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<ContractDetails>());

                // Setup Contract Repository
                _contractRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ContractId id, CancellationToken _) => CreateMockContract(id.Value));

                // Setup Pricing Repository
                _pricingRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PricingId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PricingId id, CancellationToken _) => CreateMockPricing(id.Value));

                services.AddSingleton(_contractDetailsRepoMock.Object);
                services.AddSingleton(_contractRepoMock.Object);
                services.AddSingleton(_pricingRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/contractdetails returns 200 and Guid when all required fields are provided")]
    public async Task Post_ShouldReturn200_AndId_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        ContractDetails? capturedContractDetails = null;
        _contractDetailsRepoMock.Setup(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()))
            .Callback<ContractDetails, CancellationToken>((cd, _) => capturedContractDetails = cd)
            .ReturnsAsync((ContractDetails cd, CancellationToken _) => cd);

        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var payload = new
        {
            ContractId = contractId,
            PricingId = pricingId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contractdetails", payload);

        // Debug output if test fails
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().NotBeEmpty();

        _contractDetailsRepoMock.Verify(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        _contractDetailsRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedContractDetails.Should().NotBeNull();
        capturedContractDetails!.ContractId.Value.Should().Be(contractId);
        capturedContractDetails.PricingId.Value.Should().Be(pricingId);
        capturedContractDetails.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "POST /api/contractdetails returns 400 when ContractId is empty")]
    public async Task Post_ShouldReturn400_WhenContractIdIsEmpty()
    {
        // Arrange
        var payload = new
        {
            ContractId = Guid.Empty,
            PricingId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contractdetails", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ContractId is required");

        _contractDetailsRepoMock.Verify(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contractdetails returns 400 when PricingId is empty")]
    public async Task Post_ShouldReturn400_WhenPricingIdIsEmpty()
    {
        // Arrange
        var payload = new
        {
            ContractId = Guid.NewGuid(),
            PricingId = Guid.Empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contractdetails", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("PricingId is required");

        _contractDetailsRepoMock.Verify(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contractdetails returns 409 when ContractDetails already exists")]
    public async Task Post_ShouldReturn409_WhenContractDetailsAlreadyExists()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();

        var existingContractDetails = CreateTestContractDetails(contractId, pricingId);
        _contractDetailsRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ContractDetails, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContractDetails> { existingContractDetails });

        var payload = new
        {
            ContractId = contractId,
            PricingId = pricingId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contractdetails", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"ContractDetails with ContractId {contractId} and PricingId {pricingId} already exists");

        _contractDetailsRepoMock.Verify(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contractdetails returns 404 when Contract does not exist")]
    public async Task Post_ShouldReturn404_WhenContractDoesNotExist()
    {
        // Arrange
        var nonExistentContractId = Guid.NewGuid();
        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(nonExistentContractId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        var payload = new
        {
            ContractId = nonExistentContractId,
            PricingId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contractdetails", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Contract with ID {nonExistentContractId} not found");

        _contractDetailsRepoMock.Verify(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contractdetails returns 404 when Pricing does not exist")]
    public async Task Post_ShouldReturn404_WhenPricingDoesNotExist()
    {
        // Arrange
        var nonExistentPricingId = Guid.NewGuid();
        _pricingRepoMock.Setup(r => r.GetByIdAsync(PricingId.Of(nonExistentPricingId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pricing?)null);

        var payload = new
        {
            ContractId = Guid.NewGuid(),
            PricingId = nonExistentPricingId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contractdetails", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Pricing with ID {nonExistentPricingId} not found");

        _contractDetailsRepoMock.Verify(r => r.AddAsync(It.IsAny<ContractDetails>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper Methods
    private static ContractDetails CreateTestContractDetails(Guid contractId, Guid pricingId)
    {
        return ContractDetails.Create(
            ContractDetailsId.Of(Guid.NewGuid()),
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