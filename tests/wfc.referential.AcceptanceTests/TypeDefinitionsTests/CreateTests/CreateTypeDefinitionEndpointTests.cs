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
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.CreateTests;

public class CreateTypeDefinitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();

    public CreateTypeDefinitionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TypeDefinition td, CancellationToken _) => td);

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/typedefinitions returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Libelle = "TestType",
            Description = "Test Type Definition",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/typedefinitions", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _repoMock.Verify(r =>
            r.AddTypeDefinitionAsync(It.Is<TypeDefinition>(td =>
                    td.Libelle == payload.Libelle &&
                    td.Description == payload.Description &&
                    td.IsEnabled == payload.IsEnabled),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/typedefinitions returns 400 & problem-details when Libelle is missing")]
    public async Task Post_ShouldReturn400_WhenLibelleIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            // Libelle intentionally omitted to trigger validation error
            Description = "Test Type Definition",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/typedefinitions", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("libelle")[0].GetString()
            .Should().Be("Name is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/typedefinitions returns 400 & problem-details when Description is missing")]
    public async Task Post_ShouldReturn400_WhenDescriptionIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            Libelle = "TestType",
            // Description intentionally omitted to trigger validation error
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/typedefinitions", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("description")[0].GetString()
            .Should().Be("Description is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }
}