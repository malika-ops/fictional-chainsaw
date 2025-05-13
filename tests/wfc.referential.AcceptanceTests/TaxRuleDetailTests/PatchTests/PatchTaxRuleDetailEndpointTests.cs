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
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class PatchTaxRuleDetailEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRuleDetailRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private const string BaseUrl = "api/taxruledetails";

    public PatchTaxRuleDetailEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRuleDetailRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = $"PATCH {BaseUrl}/id updates the TaxRuleDetail successfully")]
    public async Task PatchTaxRuleDetail_ShouldReturnUpdatedId_WhenTaxRuleDetailExists()
    {
        // Arrange
        var taxRuleDetailId = Guid.NewGuid();
        var patchRequest = new PatchTaxRuleDetailRequest
        {
            AppliedOn = ApplicationRule.Amount
        };

        var taxRuleDetail = TaxRuleDetail.Create(
            TaxRuleDetailsId.Of(taxRuleDetailId),
            corridorId: CorridorId.Of(Guid.NewGuid()),
            taxId: TaxId.Of(Guid.NewGuid()),
            serviceId: ServiceId.Of(Guid.NewGuid()),
            appliedOn: ApplicationRule.Fees,
            isEnabled: true);

        _repoMock.Setup(r => r.GetTaxRuleDetailByIdAsync(taxRuleDetailId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRuleDetail);

        _repoMock.Setup(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxRuleDetailId}", JsonContent.Create(patchRequest));
        var updatedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedId.Should().Be(taxRuleDetailId);

        _repoMock.Verify(r => r.GetTaxRuleDetailByIdAsync(taxRuleDetailId, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.Is<TaxRuleDetail>(trd =>
            trd.Id.Value == taxRuleDetailId &&
            trd.AppliedOn == patchRequest.AppliedOn), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = $"PATCH {BaseUrl}/id returns 404 when TaxRuleDetail does not exist")]
    public async Task PatchTaxRuleDetail_ShouldReturnNotFound_WhenTaxRuleDetailDoesNotExist()
    {
        // Arrange
        var taxRuleDetailId = Guid.NewGuid();
        var patchRequest = new PatchTaxRuleDetailRequest
        {
            CorridorId = Guid.NewGuid()
        };

        _repoMock.Setup(r => r.GetTaxRuleDetailByIdAsync(taxRuleDetailId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRuleDetail)null);

        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxRuleDetailId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.GetTaxRuleDetailByIdAsync(taxRuleDetailId, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = $"PATCH {BaseUrl}/id returns 400 when validation fails")]
    public async Task PatchTaxRuleDetail_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var taxRuleDetailId = Guid.NewGuid();
        var patchRequest = new 
        {
            CorridorId = false // Invalid
        };

        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxRuleDetailId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.GetTaxRuleDetailByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
