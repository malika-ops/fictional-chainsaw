using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.UpdateTests;

public class UpdateCurrencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICurrencyRepository> _repoMock = new();

    public UpdateCurrencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB
                services.RemoveAll<ICurrencyRepository>();

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} returns 200 and Guid when update is successful")]
    public async Task Put_ShouldReturn200_AndGuid_WhenUpdateIsSuccessful()
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

        _repoMock
            .Setup(r => r.GetByCodeAsync("EURUPDATED", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        _repoMock
            .Setup(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var updateRequest = new UpdateCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "EURUPDATED",
            CodeAR = "يورو محدث",
            CodeEN = "Updated Euro",
            Name = "Euro Updated",
            CodeIso = 978,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(currencyId);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeAsync("EURUPDATED", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} returns 400 when code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "دولار أمريكي",
            "US Dollar",
            "US Dollar",
            840
        );

        var existingCurrency = Currency.Create(
            CurrencyId.Of(existingCurrencyId),
            "EUR",
            "يورو",
            "Euro",
            "Euro",
            978
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repoMock
            .Setup(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var updateRequest = new UpdateCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "EUR",
            CodeAR = "يورو",
            CodeEN = "Euro",
            Name = "Euro Attempted",
            CodeIso = 999, // Different codeiso
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} returns 400 when codeiso already exists")]
    public async Task Put_ShouldReturn400_WhenCodeIsoAlreadyExists()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "دولار أمريكي",
            "US Dollar",
            "US Dollar",
            840
        );

        var existingCurrency = Currency.Create(
            CurrencyId.Of(existingCurrencyId),
            "EUR",
            "يورو",
            "Euro",
            "Euro",
            978
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repoMock
            .Setup(r => r.GetByCodeAsync("ABC", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        _repoMock
            .Setup(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var updateRequest = new UpdateCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "ABC", // Different code
            CodeAR = "أبجد",
            CodeEN = "ABC",
            Name = "ABC Currency",
            CodeIso = 978, // Same codeiso as existing currency
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeAsync("ABC", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} returns 400 when CodeIso is out of range")]
    public async Task Put_ShouldReturn400_WhenCodeIsoIsOutOfRange()
    {
        // Arrange
        var currencyId = Guid.NewGuid();

        var updateRequest = new UpdateCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "USD",
            CodeAR = "دولار أمريكي",
            CodeEN = "US Dollar",
            Name = "US Dollar",
            CodeIso = 1234, // More than 3 digits
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/currencies/{id} returns 400 when validation fails")]
    public async Task Put_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var currencyId = Guid.NewGuid();

        var updateRequest = new UpdateCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "", // Empty code should trigger validation
            Name = "US Dollar",
            CodeIso = 840,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/currencies/{currencyId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}