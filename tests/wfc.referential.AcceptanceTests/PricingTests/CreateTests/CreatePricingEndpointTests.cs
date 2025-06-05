using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PricingTests.CreateTests;

public class CreatePricingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPricingRepository> _pricingRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly Mock<ICorridorRepository> _corridorRepoMock = new();
    private readonly Mock<IAffiliateRepository> _affiliateRepoMock = new();

    public CreatePricingEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPricingRepository>();
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ICorridorRepository>();
                services.RemoveAll<IAffiliateRepository>();
                services.RemoveAll<ICacheService>();

                _pricingRepoMock
                    .Setup(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Pricing p, CancellationToken _) => p);

                _pricingRepoMock
                    .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                _pricingRepoMock
                    .Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pricing, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Pricing?)null);

                var dummyService = FormatterServices.GetUninitializedObject(typeof(Service)) as Service;
                var dummyCorridor = FormatterServices.GetUninitializedObject(typeof(Corridor)) as Corridor;
                var dummyAffiliate = FormatterServices.GetUninitializedObject(typeof(Affiliate)) as Affiliate;

                _serviceRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dummyService);

                _corridorRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<CorridorId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dummyCorridor);

                _affiliateRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dummyAffiliate);

                services.AddSingleton(_pricingRepoMock.Object);
                services.AddSingleton(_serviceRepoMock.Object);
                services.AddSingleton(_corridorRepoMock.Object);
                services.AddSingleton(_affiliateRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/pricings → 200 + Guid on valid request with FixedAmount")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValidWithFixedAmount()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-001",
            Channel = "Online",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _pricingRepoMock.Verify(r =>
            r.AddAsync(It.Is<Pricing>(p =>
                    p.Code == payload.Code &&
                    p.Channel == payload.Channel &&
                    p.MinimumAmount == payload.MinimumAmount &&
                    p.MaximumAmount == payload.MaximumAmount &&
                    p.FixedAmount == payload.FixedAmount &&
                    p.CorridorId.Value == payload.CorridorId &&
                    p.ServiceId.Value == payload.ServiceId),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _pricingRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/pricings → 200 + Guid on valid request with Rate")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValidWithRate()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-002",
            Channel = "Mobile",
            MinimumAmount = 100.00m,
            MaximumAmount = 2000.00m,
            Rate = 0.015m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _pricingRepoMock.Verify(r =>
            r.AddAsync(It.Is<Pricing>(p =>
                    p.Code == payload.Code &&
                    p.Channel == payload.Channel &&
                    p.Rate == payload.Rate &&
                    p.FixedAmount == null),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "POST /api/pricings → 200 + Guid on valid request with both FixedAmount and Rate")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValidWithBothFixedAmountAndRate()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-003",
            Channel = "Branch",
            MinimumAmount = 25.00m,
            MaximumAmount = 500.00m,
            FixedAmount = 2.50m,
            Rate = 0.01m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _pricingRepoMock.Verify(r =>
            r.AddAsync(It.Is<Pricing>(p =>
                    p.FixedAmount == payload.FixedAmount &&
                    p.Rate == payload.Rate),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "POST /api/pricings → 200 + Guid on valid request with AffiliateId")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValidWithAffiliateId()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-004",
            Channel = "Partner",
            MinimumAmount = 75.00m,
            MaximumAmount = 1500.00m,
            FixedAmount = 7.50m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            AffiliateId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _pricingRepoMock.Verify(r =>
            r.AddAsync(It.Is<Pricing>(p =>
                    p.AffiliateId!.Value == payload.AffiliateId),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when Code exceeds 50 chars")]
    public async Task Post_ShouldReturn400_WhenCodeTooLong()
    {
        // Arrange
        var payload = new
        {
            Code = new string('X', 51),
            Channel = "Online",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code max length = 50.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when Channel exceeds 50 chars")]
    public async Task Post_ShouldReturn400_WhenChannelTooLong()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-005",
            Channel = new string('X', 51),
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("channel")[0].GetString()
            .Should().Be("Channel max length = 50.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when MinimumAmount is not positive")]
    public async Task Post_ShouldReturn400_WhenMinimumAmountIsNotPositive()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-006",
            Channel = "Online",
            MinimumAmount = -10.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("minimumAmount")[0].GetString()
            .Should().Be("MinimumAmount must be positive.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when MaximumAmount is not greater than MinimumAmount")]
    public async Task Post_ShouldReturn400_WhenMaximumAmountIsNotGreaterThanMinimumAmount()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-007",
            Channel = "Online",
            MinimumAmount = 1000.00m,
            MaximumAmount = 500.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("maximumAmount")[0].GetString()
            .Should().Be("MaximumAmount must be strictly greater than MinimumAmount.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when neither FixedAmount nor Rate is provided")]
    public async Task Post_ShouldReturn400_WhenNeitherFixedAmountNorRateIsProvided()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-008",
            Channel = "Online",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorMessage = doc!.RootElement.GetProperty("errors")
            .EnumerateObject()
            .First().Value[0].GetString();
        errorMessage.Should().Be("Either FixedAmount or Rate must be provided (or both).");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when FixedAmount is not positive")]
    public async Task Post_ShouldReturn400_WhenFixedAmountIsNotPositive()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-009",
            Channel = "Online",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = -5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("fixedAmount")[0].GetString()
            .Should().Be("FixedAmount must be positive.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when Rate is not positive")]
    public async Task Post_ShouldReturn400_WhenRateIsNotPositive()
    {
        // Arrange
        var payload = new
        {
            Code = "PRC-010",
            Channel = "Online",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            Rate = -0.01m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("rate")[0].GetString()
            .Should().Be("Rate must be positive.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 409 when duplicate Code")]
    public async Task Post_ShouldReturn409_WhenDuplicateCode()
    {
        // Arrange
        const string duplicateCode = "DUP-PRICING";

        // Existing record returned by repo to trigger DuplicatePricingCodeException
        var existing = Pricing.Create(
            PricingId.Of(Guid.NewGuid()),
            duplicateCode,
            "Online",
            50.00m,
            1000.00m,
            5.00m,
            null,
            CorridorId.Of(Guid.NewGuid()),
            ServiceId.Of(Guid.NewGuid()),
            null);

        _pricingRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Pricing, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var payload = new
        {
            Code = duplicateCode,
            Channel = "Mobile",
            MinimumAmount = 100.00m,
            MaximumAmount = 2000.00m,
            Rate = 0.015m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 404 when Corridor does not exist")]
    public async Task Post_ShouldReturn404_WhenCorridorNotFound()
    {
        // Arrange
        _corridorRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<CorridorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Corridor?)null); // simulate missing corridor

        var payload = new
        {
            Code = "PRC-404",
            Channel = "Online",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 404 when Affiliate does not exist")]
    public async Task Post_ShouldReturn404_WhenAffiliateNotFound()
    {
        // Arrange
        _affiliateRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AffiliateId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Affiliate?)null); // simulate missing affiliate

        var payload = new
        {
            Code = "PRC-404-AFF",
            Channel = "Partner",
            MinimumAmount = 50.00m,
            MaximumAmount = 1000.00m,
            FixedAmount = 5.00m,
            CorridorId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            AffiliateId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/pricings → 400 when required fields are missing")]
    public async Task Post_ShouldReturn400_WhenRequiredFieldsAreMissing()
    {
        // Arrange
        var payload = new
        {
            // Missing Code, Channel, MinimumAmount, MaximumAmount, CorridorId, ServiceId
            FixedAmount = 5.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pricings", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = doc!.RootElement.GetProperty("errors");
        errors.GetProperty("code")[0].GetString().Should().Be("Code is required.");
        errors.GetProperty("channel")[0].GetString().Should().Be("Channel is required.");
        errors.GetProperty("serviceId")[0].GetString().Should().Be("ServiceId cannot be empty.");
        errors.GetProperty("corridorId")[0].GetString().Should().Be("CorridorId cannot be empty.");

        _pricingRepoMock.Verify(r => r.AddAsync(It.IsAny<Pricing>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
