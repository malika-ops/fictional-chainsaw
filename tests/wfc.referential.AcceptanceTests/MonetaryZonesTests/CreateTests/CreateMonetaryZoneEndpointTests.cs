using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.CreateTests;

public class CreateMonetaryZoneEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repoMock = new();

    public CreateMonetaryZoneEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // clone the factory and customise the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IMonetaryZoneRepository>();
                services.RemoveAll<ICacheService>();

                // 🪄  Set up mock behaviour (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddMonetaryZoneAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((MonetaryZone mz, CancellationToken _) => mz);


                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/monetaryZones returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Code = "USD",
            Name = "United States Dollar",
            Description = "Primary US currency"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _repoMock.Verify(r =>
            r.AddMonetaryZoneAsync(It.Is<MonetaryZone>(mz =>
                    mz.Code == payload.Code &&
                    mz.Name == payload.Name &&
                    mz.Description == payload.Description),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

    [Fact(DisplayName = "POST /api/monetaryZones returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Name = "United States Dollar",
            Description = "Primary US currency"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/monetaryZones", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        // the handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddMonetaryZoneAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

   
    [Fact(DisplayName =
    "POST /api/monetaryZones returns 400 & problem‑details when Name and Description are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndDescriptionAreMissing()
    {
        // Arrange: only Code provided
        var invalidPayload = new { Code = "USD" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/monetaryZones", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var errors = root.GetProperty("errors");

        // helper – fetch first error message for a given key, case‑insensitive
        static string FirstError(JsonElement errs, string key)
        {
            foreach (var prop in errs.EnumerateObject())
                if (prop.NameEquals(key) || prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return prop.Value[0].GetString()!;
            throw new KeyNotFoundException($"error key '{key}' not found");
        }

        FirstError(errors, "Name").Should().Be("Name is required");

        _repoMock.Verify(r => r.AddMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                     It.IsAny<CancellationToken>()),
                         Times.Never);
    }



    [Fact(DisplayName = "POST /api/monetaryZones returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "CHF";

        // Tell the repo mock that the code already exists
        var existingMz = MonetaryZone.Create(
            MonetaryZoneId.Of(Guid.NewGuid()),
            duplicateCode,
            "Swiss Franc",
            "Swiss currency",
            null);

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMz);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Swiss Franc",
            Description = "Swiss currency"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/monetaryZones", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"MonetaryZone with code {duplicateCode} already exists.");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddMonetaryZoneAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }
}
