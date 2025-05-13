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
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.CreateTests;

public class CreateParamTypeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repoMock = new();
    private readonly Mock<ITypeDefinitionRepository> _typeDefinitionRepoMock = new();

    public CreateParamTypeEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddParamTypeAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ParamType p, CancellationToken _) => p);

                // Set up typeDefinition mock to return valid entities
                var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));

                _typeDefinitionRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<TypeDefinitionId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(TypeDefinition.Create(typeDefinitionId, "TestType", "Test Type Definition"));

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_typeDefinitionRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/paramtypes returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            Value = "TestValue",
            TypeDefinitionId = typeDefinitionId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _repoMock.Verify(r =>
            r.AddParamTypeAsync(It.Is<ParamType>(p =>
                    p.Value == payload.Value &&
                    p.IsEnabled == payload.IsEnabled),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/paramtypes returns 400 & problem-details when Value is missing")]
    public async Task Post_ShouldReturn400_WhenValueIsMissing()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var invalidPayload = new
        {
            // Value intentionally omitted to trigger validation error
            TypeDefinitionId = typeDefinitionId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("value")[0].GetString()
            .Should().Be("Value is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddParamTypeAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/paramtypes returns 400 when TypeDefinition is not found")]
    public async Task Post_ShouldReturn400_WhenTypeDefinitionNotFound()
    {
        // Arrange
        var nonExistentTypeDefinitionId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Setup repository to return null for this ID
        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(TypeDefinitionId.Of(nonExistentTypeDefinitionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypeDefinition?)null);

        var payload = new
        {
            Value = "TestValue",
            TypeDefinitionId = nonExistentTypeDefinitionId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"TypeDefinition with ID {nonExistentTypeDefinitionId} not found");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddParamTypeAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}