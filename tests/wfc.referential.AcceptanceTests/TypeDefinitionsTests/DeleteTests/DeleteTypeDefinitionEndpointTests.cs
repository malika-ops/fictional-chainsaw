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

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.DeleteTests;

public class DeleteTypeDefinitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();

    public DeleteTypeDefinitionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

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

    // Helper to build test TypeDefinition
    private static TypeDefinition CreateTestTypeDefinition(Guid id, string libelle, string description, bool withParamTypes = false)
    {
        var typeDefinition = TypeDefinition.Create(
            new TypeDefinitionId(id),
            libelle,
            description
        );

        if (withParamTypes)
        {
            var paramType = ParamType.Create(
                new ParamTypeId(Guid.NewGuid()),
                typeDefinition.Id,
                "Test Param Value"
            );
            typeDefinition.AddParamType(paramType);
        }

        return typeDefinition;
    }

    [Fact(DisplayName = "DELETE /api/typedefinitions/{id} returns 200 when typeDefinition exists and has no paramTypes")]
    public async Task Delete_ShouldReturn200_WhenTypeDefinitionExistsAndHasNoParamTypes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Type", "Test Description");

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDefinition);

        // Capture the entity passed to Update
        TypeDefinition? updatedTypeDefinition = null;
        _repoMock
            .Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()))
            .Callback<TypeDefinition, CancellationToken>((td, _) => updatedTypeDefinition = td)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/typedefinitions/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedTypeDefinition!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                        It.IsAny<CancellationToken>()),
                                                        Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/typedefinitions/{id} returns 400 when typeDefinition is not found")]
    public async Task Delete_ShouldReturn400_WhenTypeDefinitionNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypeDefinition?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/typedefinitions/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Type definition not found");

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                        It.IsAny<CancellationToken>()),
                                                        Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/typedefinitions/{id} returns 400 when typeDefinition has linked paramTypes")]
    public async Task Delete_ShouldReturn400_WhenTypeDefinitionHasLinkedParamTypes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Type", "Test Description", true); // With paramTypes

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDefinition);

        // Act
        var response = await _client.DeleteAsync($"/api/typedefinitions/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Cannot delete TypeDefinition with ID {id} because it is linked to one or more ParamTypes.");

        _repoMock.Verify(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                        It.IsAny<CancellationToken>()),
                                                        Times.Never);
    }
}