using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class CreateTaxRuleDetailEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRuleDetailRepository> _repoMock = new();
    private readonly Mock<ICorridorRepository> _repoCorridorMock = new();
    private readonly Mock<ITaxRepository> _repoTaxMock = new();
    private readonly Mock<IServiceRepository> _repoServiceMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private const string BaseUrl = "api/tax-rule-details";

    public CreateTaxRuleDetailEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRuleDetailRepository>();
                services.RemoveAll<ICorridorRepository>();
                services.RemoveAll<ITaxRepository>();
                services.RemoveAll<IServiceRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaxRuleDetail trd, CancellationToken _) => trd);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_repoCorridorMock.Object);
                services.AddSingleton(_repoTaxMock.Object);
                services.AddSingleton(_repoServiceMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 201 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndGuid_WhenRequestIsValid()
    {
        // Arrange
        var payload = new CreateTaxRuleDetailRequest
        {
            CorridorId = Guid.NewGuid(),
            TaxId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            AppliedOn = ApplicationRule.Amount
        };

        _repoCorridorMock.Setup(r =>
            r.GetByIdAsync(It.Is<CorridorId>(id => id.Value == payload.CorridorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Corridor.Create(CorridorId.Create(),CountryId.Of(Guid.NewGuid()),
            CountryId.Of(Guid.NewGuid()),CityId.Create(), CityId.Create(),
            AgencyId.Of(Guid.NewGuid()),AgencyId.Of(Guid.NewGuid())));
        _repoTaxMock.Setup(r =>
            r.GetByIdAsync(It.Is<TaxId>(id => id.Value == payload.TaxId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tax.Create(TaxId.Create(),"code","codeEn","codeAR", "Test Tax", 20, 10));
        _repoServiceMock.Setup(r =>
            r.GetByIdAsync(It.Is<ServiceId>(id => id.Value == payload.ServiceId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Service.Create(ServiceId.Of(Guid.NewGuid()), "Test Service", "name",true, ProductId.Of(Guid.NewGuid())));

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<TaxRuleDetail>(trd =>
                trd.CorridorId.Value == payload.CorridorId &&
                trd.TaxId.Value == payload.TaxId &&
                trd.ServiceId.Value == payload.ServiceId &&
                trd.AppliedOn == payload.AppliedOn &&
                trd.IsEnabled == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 400 when required fields are missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange: missing CorridorId and ServiceId
        var invalidPayload = new CreateTaxRuleDetailRequest
        {
            TaxId = Guid.NewGuid(),
            AppliedOn = ApplicationRule.Fees
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");
        errors.TryGetProperty("CorridorId", out var corridorErrors).Should().BeTrue();
        corridorErrors[0].GetString().Should().Contain("CorridorId is required");

        errors.TryGetProperty("ServiceId", out var serviceErrors).Should().BeTrue();
        serviceErrors[0].GetString().Should().Contain("ServiceId is required");

        // Verify repo was never called
        _repoMock.Verify(r =>
            r.AddAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
