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
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.RegionTests.CreateTests;

public class CreateRegionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repoMock = new();

    public CreateRegionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // clone the factory and customise the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IRegionRepository>();
                services.RemoveAll<ICacheService>();

                // 🪄  Set up mock behaviour (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddRegionAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Region r, CancellationToken _) => r);

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/regions returns 200 and Guid (fixture version)")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Code = "999",
            Name = "Casablanca-Settat",
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/regions", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // verify repository interaction using *FluentAssertions on Moq invocations
        _repoMock.Verify(r =>
            r.AddRegionAsync(It.Is<Region>(r =>
                    r.Code == payload.Code &&
                    r.Name == payload.Name &&
                    r.CountryId == CountryId.Of(payload.CountryId)),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

    [Fact(DisplayName = "POST /api/regions returns 400 & problem‑details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Name = "United States Dollar",
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/regions", invalidPayload);
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
            r.AddRegionAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/regions returns 400 & problem‑details when Name and Code are missing")]
    public async Task Post_ShouldReturn400_WhenNameAndCodeAreMissing()
    {
        // ---------- Arrange ----------
        var invalidPayload = new
        {
            CountryId = Guid.Parse("50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1")
        };

        // ---------- Act ----------
        var response = await _client.PostAsJsonAsync("/api/regions", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // ---------- Assert ----------
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;

        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");

        errors.GetProperty("name")[0].GetString()
              .Should().Be("Name is required");

        errors.GetProperty("code")[0].GetString()
              .Should().Be("Code is required");

        // handler must NOT run on validation failure
        _repoMock.Verify(r =>
            r.AddRegionAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/regions returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "99";

        // Tell the repo mock that the code already exists
        var region = Region.Create(
            RegionId.Of(Guid.NewGuid()),
            duplicateCode,
            "Swiss Franc",
            CountryId.Of(Guid.NewGuid()));

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(region);

        var payload = new
        {
            Code = duplicateCode,
            Name = "Swiss Franc",
            CountryId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/regions", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Region with code : {duplicateCode} already exist");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddRegionAsync(It.IsAny<Region>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }
}
