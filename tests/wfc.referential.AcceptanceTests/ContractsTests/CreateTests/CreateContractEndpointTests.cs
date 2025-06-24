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

namespace wfc.referential.AcceptanceTests.ContractsTests.CreateTests;

public class CreateContractEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IContractRepository> _contractRepoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();

    public CreateContractEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IContractRepository>();
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ICacheService>();

                // Setup Contract Repository
                _contractRepoMock.Setup(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Contract c, CancellationToken _) => c);
                _contractRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Contract>());

                // Setup Partner Repository
                _partnerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PartnerId id, CancellationToken _) => CreateMockPartner(id.Value));

                services.AddSingleton(_contractRepoMock.Object);
                services.AddSingleton(_partnerRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/contracts returns 200 and Guid when all required fields are provided")]
    public async Task Post_ShouldReturn200_AndId_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        Contract? capturedContract = null;
        _contractRepoMock.Setup(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
            .Callback<Contract, CancellationToken>((c, _) => capturedContract = c)
            .ReturnsAsync((Contract c, CancellationToken _) => c);

        var partnerId = Guid.NewGuid();
        var payload = new
        {
            Code = "CTR001",
            PartnerId = partnerId,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(365)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

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

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Once);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedContract.Should().NotBeNull();
        capturedContract!.Code.Should().Be("CTR001");
        capturedContract.PartnerId.Value.Should().Be(partnerId);
        capturedContract.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "POST /api/contracts returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var payload = new
        {
            // Code intentionally omitted
            PartnerId = Guid.NewGuid(),
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(365)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts returns 400 when PartnerId is empty")]
    public async Task Post_ShouldReturn400_WhenPartnerIdIsEmpty()
    {
        // Arrange
        var payload = new
        {
            Code = "CTR001",
            PartnerId = Guid.Empty,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(365)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("PartnerId is required");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts returns 400 when EndDate is before StartDate")]
    public async Task Post_ShouldReturn400_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var payload = new
        {
            Code = "CTR001",
            PartnerId = Guid.NewGuid(),
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(5) // EndDate before StartDate
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("EndDate must be greater than StartDate");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        const string duplicateCode = "CTR001";

        var existingContract = CreateTestContract(duplicateCode, Guid.NewGuid());
        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract> { existingContract });

        var payload = new
        {
            Code = duplicateCode,
            PartnerId = Guid.NewGuid(),
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(365)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Contract with code {duplicateCode} already exists");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts returns 404 when Partner does not exist")]
    public async Task Post_ShouldReturn404_WhenPartnerDoesNotExist()
    {
        // Arrange
        var nonExistentPartnerId = Guid.NewGuid();
        _partnerRepoMock.Setup(r => r.GetByIdAsync(PartnerId.Of(nonExistentPartnerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var payload = new
        {
            Code = "CTR001",
            PartnerId = nonExistentPartnerId,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(365)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Partner with ID {nonExistentPartnerId} not found");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts validates StartDate is required")]
    public async Task Post_ShouldReturn400_WhenStartDateIsDefault()
    {
        // Arrange
        var payload = new
        {
            Code = "CTR001",
            PartnerId = Guid.NewGuid(),
            StartDate = default(DateTime), // Default DateTime
            EndDate = DateTime.Today.AddDays(365)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("StartDate is required");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts validates EndDate is required")]
    public async Task Post_ShouldReturn400_WhenEndDateIsDefault()
    {
        // Arrange
        var payload = new
        {
            Code = "CTR001",
            PartnerId = Guid.NewGuid(),
            StartDate = DateTime.Today.AddDays(1),
            EndDate = default(DateTime) // Default DateTime
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("EndDate is required");

        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/contracts accepts valid date ranges")]
    public async Task Post_ShouldAcceptValidDateRanges()
    {
        // Arrange
        _contractRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Contract, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract>());

        var payload = new
        {
            Code = "CTR001",
            PartnerId = Guid.NewGuid(),
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contracts", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _contractRepoMock.Verify(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper Methods
    private static Contract CreateTestContract(string code, Guid partnerId)
    {
        return Contract.Create(
            ContractId.Of(Guid.NewGuid()),
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