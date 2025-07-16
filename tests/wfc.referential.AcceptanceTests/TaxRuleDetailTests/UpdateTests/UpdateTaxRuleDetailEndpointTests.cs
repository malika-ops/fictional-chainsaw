using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
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

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests.UpdateTests;

public class UpdateTaxRuleDetailEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl = "api/tax-rule-details";

    private static TaxRuleDetail CreateDummyTaxRuleDetail(Guid id, Guid corridorId, Guid taxId, Guid serviceId) =>
        TaxRuleDetail.Create(
            TaxRuleDetailsId.Of(id),
            CorridorId.Of(corridorId),
            TaxId.Of(taxId),
            ServiceId.Of(serviceId),
            ApplicationRule.Fees,
            isEnabled: true);

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var corridorId = Guid.NewGuid();
        var taxId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var oldTaxRuleDetail = CreateDummyTaxRuleDetail(id, corridorId, taxId, serviceId);

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTaxRuleDetail);

        _taxRuleDetailsRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TaxRuleDetail)null); // No duplicate

        _corridorRepoMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<CorridorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Corridor.Create(CorridorId.Create(), CountryId.Of(Guid.NewGuid()),
            CountryId.Of(Guid.NewGuid()), CityId.Create(), CityId.Create(),
            AgencyId.Of(Guid.NewGuid()), AgencyId.Of(Guid.NewGuid())));

        _taxRepoMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<TaxId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tax.Create(TaxId.Create(), "code", "codeEn", "codeAR", "Test Tax", 20, 10));

        _serviceRepoMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Service.Create(ServiceId.Of(Guid.NewGuid()), "Test Service", "name", FlowDirection.Debit, true, ProductId.Of(Guid.NewGuid())));


        TaxRuleDetail? updated = null;
        _taxRuleDetailsRepoMock.Setup(r => r.Update(oldTaxRuleDetail))
                 .Callback<TaxRuleDetail>((trd) => updated = trd);

        var payload = new UpdateTaxRuleDetailRequest
        {
            CorridorId = corridorId,
            TaxId = taxId,
            ServiceId = serviceId,
            AppliedOn = ApplicationRule.Amount,
            IsEnabled = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().Be(true);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 400 when validation fails")]
    public async Task Put_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Missing CorridorId (Guid.Empty), which is invalid
        var payload = new UpdateTaxRuleDetailRequest
        {
            CorridorId= Guid.Parse("44dc44f7-b825-4fea-91d2-4582524d8af5"),
            TaxId= Guid.Parse("dd4e7315-63ba-4631-96a2-794653236842"),
            ServiceId= Guid.Parse("d8f33651-403d-4fdb-9c60-5e1ccfb3ae06"),
            IsEnabled= false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _taxRuleDetailsRepoMock.Verify(r => r.Update(It.IsAny<TaxRuleDetail>()), Times.Never);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 404 when TaxRuleDetail does not exist")]
    public async Task Put_ShouldReturn404_WhenTaxRuleDetailDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TaxRuleDetail)null);

        var payload = new UpdateTaxRuleDetailRequest
        {
            CorridorId = Guid.NewGuid(),
            TaxId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            AppliedOn = ApplicationRule.Amount,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _taxRuleDetailsRepoMock.Verify(r => r.Update(It.IsAny<TaxRuleDetail>()), Times.Never);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 409 when duplicate TaxRuleDetail exists")]
    public async Task Put_ShouldReturn409_WhenDuplicateExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var corridorId = Guid.NewGuid();
        var taxId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var existingTaxRuleDetail = CreateDummyTaxRuleDetail(Guid.NewGuid(), corridorId, taxId, serviceId);
        var oldTaxRuleDetail = CreateDummyTaxRuleDetail(id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _taxRuleDetailsRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<TaxRuleDetailsId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTaxRuleDetail);

        _taxRuleDetailsRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<Expression<Func<TaxRuleDetail, bool>>>(),
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTaxRuleDetail);


        _corridorRepoMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<CorridorId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Corridor.Create(CorridorId.Create(), CountryId.Of(Guid.NewGuid()),
            CountryId.Of(Guid.NewGuid()), CityId.Create(), CityId.Create(),
            AgencyId.Of(Guid.NewGuid()), AgencyId.Of(Guid.NewGuid())));

        _taxRepoMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<TaxId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tax.Create(TaxId.Create(), "code", "codeEn", "codeAR", "Test Tax", 20, 10));

        _serviceRepoMock.Setup(r =>
            r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Service.Create(ServiceId.Of(Guid.NewGuid()), "Test Service", "name", FlowDirection.Debit, true, ProductId.Of(Guid.NewGuid())));


        var payload = new UpdateTaxRuleDetailRequest
        {
            CorridorId = corridorId,
            TaxId = taxId,
            ServiceId = serviceId,
            AppliedOn = ApplicationRule.Amount,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _taxRuleDetailsRepoMock.Verify(r => r.Update(It.IsAny<TaxRuleDetail>()), Times.Never);
    }
}
