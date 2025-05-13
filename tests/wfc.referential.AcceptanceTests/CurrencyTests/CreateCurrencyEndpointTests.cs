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
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.CreateTests;

public class CreateCurrencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICurrencyRepository> _repoMock = new();

    public CreateCurrencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ICurrencyRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Currency c, CancellationToken _) => c);

                // GetByCodeAsync should return null for valid test cases
                _repoMock
                    .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Currency)null);

                // GetByCodeIsoAsync should return null for valid test cases
                _repoMock
                    .Setup(r => r.GetByCodeIsoAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Currency)null);

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/currencies returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new CreateCurrencyRequest
        {
            Code = "USD",
            CodeAR = "دولار أمريكي",
            CodeEN = "US Dollar",
            Name = "United States Dollar",
            CodeIso = 840
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _repoMock.Verify(r =>
            r.AddCurrencyAsync(It.Is<Currency>(c =>
                    c.Code == payload.Code &&
                    c.CodeAR == payload.CodeAR &&
                    c.CodeEN == payload.CodeEN &&
                    c.Name == payload.Name &&
                    c.CodeIso == payload.CodeIso &&
                    c.IsEnabled == true),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 & problem-details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var invalidPayload = new CreateCurrencyRequest
        {
            // Code intentionally omitted to trigger validation error
            Name = "United States Dollar",
            CodeIso = 840
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 & problem-details when Name is missing")]
    public async Task Post_ShouldReturn400_WhenNameIsMissing()
    {
        // Arrange
        var invalidPayload = new CreateCurrencyRequest
        {
            Code = "USD",
            // Name intentionally omitted to trigger validation error
            CodeIso = 840
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when CodeIso is out of range")]
    public async Task Post_ShouldReturn400_WhenCodeIsoIsOutOfRange()
    {
        // Arrange
        var invalidPayload = new CreateCurrencyRequest
        {
            Code = "USD",
            Name = "United States Dollar",
            CodeAR = "دولار أمريكي",
            CodeEN = "US Dollar",
            CodeIso = 1234, // More than 3 digits
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when duplicate code is provided")]
    public async Task Post_ShouldReturn400_WhenDuplicateCodeIsProvided()
    {
        // Arrange
        var existingCurrency = Currency.Create(
            CurrencyId.Of(Guid.NewGuid()),
            "EUR",
            "يورو",
            "Euro",
            "Euro",
            978
        );

        // Setup the repository to return an existing currency with the same code
        _repoMock
            .Setup(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var payload = new CreateCurrencyRequest
        {
            Code = "EUR", // Using same code as existing currency
            CodeAR = "يورو",
            CodeEN = "Euro",
            Name = "European Currency",
            CodeIso = 978
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The repository should be checked for an existing currency with that code
        _repoMock.Verify(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()), Times.Once);

        // But no new currency should be added
        _repoMock.Verify(r => r.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/currencies returns 400 when duplicate codeiso is provided")]
    public async Task Post_ShouldReturn400_WhenDuplicateCodeIsoIsProvided()
    {
        // Arrange
        var existingCurrency = Currency.Create(
            CurrencyId.Of(Guid.NewGuid()),
            "EUR",
            "يورو",
            "Euro",
            "Euro",
            978
        );

        // Setup the repository to return an existing currency with the same codeiso
        _repoMock
            .Setup(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCurrency);

        var payload = new CreateCurrencyRequest
        {
            Code = "USD", // Different code
            CodeAR = "دولار أمريكي",
            CodeEN = "US Dollar",
            Name = "United States Dollar",
            CodeIso = 978 // Same codeiso as existing currency
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/currencies", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The repository should be checked for an existing currency with that codeiso
        _repoMock.Verify(r => r.GetByCodeIsoAsync(978, It.IsAny<CancellationToken>()), Times.Once);

        // But no new currency should be added
        _repoMock.Verify(r => r.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}