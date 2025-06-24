using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
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

public class PatchTaxRuleDetailEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRuleDetailRepository> _repoMock = new();
    private readonly Mock<ICorridorRepository> _repoCorridorMock = new();
    private readonly Mock<ITaxRepository> _repoTaxMock = new();
    private readonly Mock<IServiceRepository> _repoServiceMock = new();
    private const string BaseUrl = "api/tax-rule-details";

    public PatchTaxRuleDetailEndpointTests(WebApplicationFactory<Program> factory)
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

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_repoCorridorMock.Object);
                services.AddSingleton(_repoTaxMock.Object);
                services.AddSingleton(_repoServiceMock.Object);
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

        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxRuleDetail);

        _repoCorridorMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<CorridorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Corridor.Create(CorridorId.Create(), CountryId.Of(Guid.NewGuid()),
            CountryId.Of(Guid.NewGuid()), CityId.Create(), CityId.Create(),
            AgencyId.Of(Guid.NewGuid()), AgencyId.Of(Guid.NewGuid())));

        _repoTaxMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<TaxId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tax.Create(TaxId.Create(), "code", "codeEn", "codeAR", "Test Tax", 20, 10));

        _repoServiceMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Service.Create(ServiceId.Of(Guid.NewGuid()), "Test Service", "name", true, ProductId.Of(Guid.NewGuid())));

        _repoMock.Setup(r => r.Update(It.IsAny<TaxRuleDetail>()));


        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxRuleDetailId}", JsonContent.Create(patchRequest));
        var updatedId = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedId.Should().Be(true);

        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.Update(It.Is<TaxRuleDetail>(trd =>
            trd.Id.Value == taxRuleDetailId &&
            trd.AppliedOn == patchRequest.AppliedOn)), Times.Once);
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

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaxRuleDetail)null);

        // Act
        var response = await _client.PatchAsync($"{BaseUrl}/{taxRuleDetailId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.Update(It.IsAny<TaxRuleDetail>()), Times.Never);
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

        _repoMock.Verify(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.Update(It.IsAny<TaxRuleDetail>()), Times.Never);
    }
}
