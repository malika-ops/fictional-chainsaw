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
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class UpdateTaxRuleDetailEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRuleDetailRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private const string BaseUrl = "api/taxruledetails";

    public UpdateTaxRuleDetailEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRuleDetailRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

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

        _repoMock.Setup(r => r.GetTaxRuleDetailByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTaxRuleDetail);

        _repoMock.Setup(r => r.GetByCorridorTaxServiceAsync(It.IsAny<CorridorId>(), It.IsAny<TaxId>(), It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TaxRuleDetail)null); // No duplicate

        TaxRuleDetail? updated = null;
        _repoMock.Setup(r => r.UpdateTaxRuleDetailAsync(oldTaxRuleDetail, It.IsAny<CancellationToken>()))
                 .Callback<TaxRuleDetail, CancellationToken>((trd, _) => updated = trd)
                 .Returns(Task.CompletedTask);

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
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().Be(id);

        updated.Should().NotBeNull();
        updated!.CorridorId.Value.Should().Be(payload.CorridorId);
        updated.TaxId.Value.Should().Be(payload.TaxId);
        updated.ServiceId.Value.Should().Be(payload.ServiceId);
        updated.AppliedOn.Should().Be(payload.AppliedOn);
        updated.IsEnabled.Should().Be(payload.IsEnabled);

        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()), Times.Once);
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

        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 404 when TaxRuleDetail does not exist")]
    public async Task Put_ShouldReturn404_WhenTaxRuleDetailDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetTaxRuleDetailByIdAsync(id, It.IsAny<CancellationToken>()))
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

        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 400 when duplicate TaxRuleDetail exists")]
    public async Task Put_ShouldReturn400_WhenDuplicateExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var corridorId = Guid.NewGuid();
        var taxId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var existingTaxRuleDetail = CreateDummyTaxRuleDetail(Guid.NewGuid(), corridorId, taxId, serviceId);
        var oldTaxRuleDetail = CreateDummyTaxRuleDetail(id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _repoMock.Setup(r => r.GetTaxRuleDetailByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTaxRuleDetail);

        _repoMock.Setup(r => r.GetByCorridorTaxServiceAsync(
            It.Is<CorridorId>(c => c.Value == corridorId),
            It.Is<TaxId>(t => t.Value == taxId),
            It.Is<ServiceId>(s => s.Value == serviceId),
            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingTaxRuleDetail);

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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors").GetString().Should().Contain("already exist");

        _repoMock.Verify(r => r.UpdateTaxRuleDetailAsync(It.IsAny<TaxRuleDetail>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
