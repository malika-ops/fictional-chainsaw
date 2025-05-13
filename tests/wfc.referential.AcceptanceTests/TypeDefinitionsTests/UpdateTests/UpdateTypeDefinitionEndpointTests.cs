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

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.UpdateTests;

public class UpdateTypeDefinitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();

    public UpdateTypeDefinitionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                           It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    // Helper to create a test TypeDefinition
    private static TypeDefinition CreateTestTypeDefinition(Guid id, string libelle, string description)
    {
        return TypeDefinition.Create(
            new TypeDefinitionId(id),
            libelle,
            description
        );
    }

    [Fact(DisplayName = "PUT /api/typedefinitions/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldTypeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTypeDefinition);

        TypeDefinition? updated = null;
        _repoMock.Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<TypeDefinition, CancellationToken>((td, _) => updated = td)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle",
            Description = "New Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/typedefinitions/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Libelle.Should().Be("New Libelle");
        updated.Description.Should().Be("New Description");
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/typedefinitions/{id} handles disabling correctly")]
    public async Task Put_ShouldHandleDisabling_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldTypeDefinition = CreateTestTypeDefinition(id, "Test Libelle", "Test Description");
        // By default it's enabled

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTypeDefinition);

        TypeDefinition? updated = null;
        _repoMock.Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<TypeDefinition, CancellationToken>((td, _) => updated = td)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "Test Libelle",
            Description = "Test Description",
            IsEnabled = false  // We want to disable it
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/typedefinitions/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        // Verify Disable was called (test if events were raised correctly)
        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(
                                It.Is<TypeDefinition>(td =>
                                    td.IsEnabled == false &&
                                    td.Libelle == "Test Libelle" &&
                                    td.Description == "Test Description"),
                                It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/typedefinitions/{id} returns 400 when Libelle is missing")]
    public async Task Put_ShouldReturn400_WhenLibelleMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            TypeDefinitionId = id,
            // Libelle omitted
            Description = "New Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/typedefinitions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("libelle")[0].GetString()
            .Should().Be("Libelle is required");

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/typedefinitions/{id} returns 400 when Description is missing")]
    public async Task Put_ShouldReturn400_WhenDescriptionMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle",
            // Description omitted
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/typedefinitions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("description")[0].GetString()
            .Should().Be("Description is required");

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/typedefinitions/{id} returns 404 when typeDefinition doesn't exist")]
    public async Task Put_ShouldReturn404_WhenTypeDefinitionDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle",
            Description = "New Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/typedefinitions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"TypeDefinition with ID {id} not found");

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}