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
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.UpdateTests;

public class UpdateTypeDefinitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    public UpdateTypeDefinitionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.Update(It.IsAny<TypeDefinition>()));

                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
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

    [Fact(DisplayName = "PUT /api/type-definitions/{id} returns 200 and bool when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldTypeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTypeDefinition);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        TypeDefinition? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<TypeDefinition>()))
                 .Callback<TypeDefinition>(td => updated = td);

        var payload = new UpdateTypeDefinitionRequest
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle",
            Description = "New Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/type-definitions/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.Libelle.Should().Be("New Libelle");
        updated.Description.Should().Be("New Description");
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/type-definitions/{id} returns 409 when Libelle already exists")]
    public async Task Put_ShouldReturn409_WhenLibelleAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var duplicateId = Guid.NewGuid();
        var existingLibelle = "Existing Libelle";

        var oldTypeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");
        var duplicateTypeDefinition = CreateTestTypeDefinition(duplicateId, existingLibelle, "Other");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTypeDefinition);

        // Setup duplicate check to return existing TypeDefinition
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicateTypeDefinition);

        var payload = new UpdateTypeDefinitionRequest
        {
            TypeDefinitionId = id,
            Libelle = existingLibelle, // Already used
            Description = "New Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/type-definitions/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/type-definitions/{id} handles disabling correctly")]
    public async Task Put_ShouldHandleDisabling_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldTypeDefinition = CreateTestTypeDefinition(id, "Test Libelle", "Test Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTypeDefinition);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        TypeDefinition? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<TypeDefinition>()))
                 .Callback<TypeDefinition>(td => updated = td);

        var payload = new UpdateTypeDefinitionRequest
        {
            TypeDefinitionId = id,
            Libelle = "Test Libelle",
            Description = "Test Description",
            IsEnabled = false  // We want to disable it
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/type-definitions/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Expecting bool return

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.Update(
                                It.Is<TypeDefinition>(td =>
                                    td.IsEnabled == false &&
                                    td.Libelle == "Test Libelle" &&
                                    td.Description == "Test Description")),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/type-definitions/{id} returns 400 when Libelle is missing")]
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
        var response = await _client.PutAsJsonAsync($"/api/type-definitions/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/type-definitions/{id} returns 400 when Description is missing")]
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
        var response = await _client.PutAsJsonAsync($"/api/type-definitions/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/type-definitions/{id} returns 400 when typeDefinition doesn't exist")]
    public async Task Put_ShouldReturn400_WhenTypeDefinitionDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        var payload = new UpdateTypeDefinitionRequest
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle",
            Description = "New Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/type-definitions/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }
}