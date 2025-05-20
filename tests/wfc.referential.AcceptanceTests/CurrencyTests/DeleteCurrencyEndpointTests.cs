using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.DeleteTests;

public class DeleteCurrencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICurrencyRepository> _repoMock = new();

    public DeleteCurrencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ICurrencyRepository>();

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} returns 200 and true when currency is set to inactive")]
    public async Task Delete_ShouldReturn200_AndTrue_WhenCurrencyIsSetToInactive()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "دولار أمريكي",
            "US Dollar",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        // Act
        var response = await _client.DeleteAsync($"/api/currencies/{currencyId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);

        // Vérifier que la mise à jour est appelée (pour changer le statut)
        _repoMock.Verify(r => r.UpdateCurrencyAsync(
            It.Is<Currency>(c =>
                c.IsEnabled == false &&
                c.CodeIso == 840), // Vérifier que le numéro reste le même
            It.IsAny<CancellationToken>()
        ), Times.Once);

        // Vérifier que la suppression n'est jamais appelée
        _repoMock.Verify(r => r.DeleteCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} returns 400 when ID is invalid")]
    public async Task Delete_ShouldReturn400_WhenIdIsInvalid()
    {
        // Arrange & Act
        var response = await _client.DeleteAsync("/api/currencies/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/currencies/{id} returns 400 when currency is associated with countries")]
    public async Task Delete_ShouldReturn400_WhenCurrencyIsAssociatedWithCountries()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "دولار أمريكي",
            "US Dollar",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        // Mock the check for associated countries to return true
        _repoMock
            .Setup(r => r.IsCurrencyAssociatedWithCountryAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/currencies/{currencyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.IsCurrencyAssociatedWithCountryAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);

        // Verify the currency was NOT updated since it's associated with countries
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}