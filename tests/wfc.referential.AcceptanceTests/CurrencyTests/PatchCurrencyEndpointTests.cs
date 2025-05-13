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

namespace wfc.referential.AcceptanceTests.CurrencyTests.PatchTests;

public class PatchCurrencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICurrencyRepository> _repoMock = new();

    public PatchCurrencyEndpointTests(WebApplicationFactory<Program> factory)
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

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 200 and Guid when patch is successful")]
    public async Task Patch_ShouldReturn200_AndGuid_WhenPatchIsSuccessful()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            Name = "Updated US Dollar", // Only changing the name
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(currencyId);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 200 and Guid when only code is patched")]
    public async Task Patch_ShouldReturn200_AndGuid_WhenOnlyCodeIsPatched()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repoMock
            .Setup(r => r.GetByCodeAsync("USDNEW", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "USDNEW", // Only changing the code
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(currencyId);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeAsync("USDNEW", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 200 and Guid when only codeiso is patched")]
    public async Task Patch_ShouldReturn200_AndGuid_WhenOnlyCodeIsoIsPatched()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repoMock
            .Setup(r => r.GetByCodeIsoAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency)null);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            CodeIso = 999, // Only changing the codeiso
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(currencyId);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeIsoAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 200 and Guid when only IsEnabled is patched")]
    public async Task Patch_ShouldReturn200_AndGuid_WhenOnlyIsEnabledIsPatched()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            IsEnabled = false, // Only changing the enabled status
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);
        var result = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(currencyId);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 400 when patched code already exists")]
    public async Task Patch_ShouldReturn400_WhenPatchedCodeAlreadyExists()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        var existingCurrency = Currency.Create(
            CurrencyId.Of(existingCurrencyId),
            "EUR",
            "Euro",
            "يورو",
            "Euro",
            978
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repoMock
            .Setup(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            Code = "EUR", // Code already exists for another currency
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 400 when patched codeiso already exists")]
    public async Task Patch_ShouldReturn400_WhenPatchedCodeIsoAlreadyExists()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var existingCurrencyId = Guid.NewGuid();

        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        var existingCurrency = Currency.Create(
            CurrencyId.Of(existingCurrencyId),
            "EUR",
            "Euro",
            "يورو",
            "Euro",
            978
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repoMock
            .Setup(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            CodeIso = 978, // CodeIso already exists for another currency
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/currencies/{id} returns 400 when codeiso is out of range")]
    public async Task Patch_ShouldReturn400_WhenCodeIsoIsOutOfRange()
    {
        // Arrange
        var currencyId = Guid.NewGuid();
        var currency = Currency.Create(
            CurrencyId.Of(currencyId),
            "USD",
            "US Dollar",
            "دولار أمريكي",
            "US Dollar",
            840
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var patchRequest = new PatchCurrencyRequest
        {
            CurrencyId = currencyId,
            CodeIso = 1234, // More than 3 digits
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/currencies/{currencyId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify repository interactions
        _repoMock.Verify(r => r.GetByIdAsync(It.Is<CurrencyId>(id => id.Value == currencyId), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.UpdateCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}