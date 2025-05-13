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
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class CreateTaxRuleDetailEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRuleDetailRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private const string BaseUrl = "api/taxruledetails";

    public CreateTaxRuleDetailEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRuleDetailRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.AddTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaxRuleDetail trd, CancellationToken _) => trd);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 200 and Guid when request is valid")]
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

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddTaxRuleDetailAsync(It.Is<TaxRuleDetail>(trd =>
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
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");
        errors.TryGetProperty("corridorId", out var corridorErrors).Should().BeTrue();
        corridorErrors[0].GetString().Should().Contain("CorridorId is required");

        errors.TryGetProperty("serviceId", out var serviceErrors).Should().BeTrue();
        serviceErrors[0].GetString().Should().Contain("ServiceId is required");

        // Verify repo was never called
        _repoMock.Verify(r =>
            r.AddTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
