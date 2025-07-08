using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.PatchTests;

public class PatchTypeDefinitionEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    // Helper to create a test TypeDefinition
    private static TypeDefinition CreateTestTypeDefinition(Guid id, string libelle, string description)
    {
        return TypeDefinition.Create(
            new TypeDefinitionId(id),
            libelle,
            description
        );
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} returns 200 and bool when patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        // Setup duplicate check to return null (no duplicates)
        _typeDefinitionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        TypeDefinition? updated = null;
        _typeDefinitionRepoMock.Setup(r => r.Update(It.IsAny<TypeDefinition>()))
                 .Callback<TypeDefinition>(td => updated = td);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle"
            // IsEnabled and Description intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.Libelle.Should().Be("New Libelle");
        updated.Description.Should().Be("Old Description"); // Should not change
        updated.IsEnabled.Should().BeTrue(); // Should remain true

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} returns 200 when updating enabled status")]
    public async Task Patch_ShouldReturn200_WhenUpdatingEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Libelle", "Test Description");

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        // Setup duplicate check to return null (no duplicates)
        _typeDefinitionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        TypeDefinition? updated = null;
        _typeDefinitionRepoMock.Setup(r => r.Update(It.IsAny<TypeDefinition>()))
                 .Callback<TypeDefinition>(td => updated = td);

        var payload = new
        {
            TypeDefinitionId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.IsEnabled.Should().BeFalse();
        updated.Libelle.Should().Be("Test Libelle"); // Should not change
        updated.Description.Should().Be("Test Description"); // Should not change

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} returns 404 when typeDefinition doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenTypeDefinitionDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "New Libelle"
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Updated to expect 404 like Bank pattern

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} returns 400 when Libelle is empty")]
    public async Task Patch_ShouldReturn400_WhenLibelleIsEmpty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = ""  // Empty libelle
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} returns 400 when Description is empty")]
    public async Task Patch_ShouldReturn400_WhenDescriptionIsEmpty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        var payload = new
        {
            TypeDefinitionId = id,
            Description = ""  // Empty description
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} handles both libelle and description changes")]
    public async Task Patch_ShouldHandleBothLibelleAndDescriptionChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        // Setup duplicate check to return null (no duplicates)
        _typeDefinitionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        TypeDefinition? updated = null;
        _typeDefinitionRepoMock.Setup(r => r.Update(It.IsAny<TypeDefinition>()))
                 .Callback<TypeDefinition>(td => updated = td);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "Updated Libelle",
            Description = "Updated Description",
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.Libelle.Should().Be("Updated Libelle");
        updated.Description.Should().Be("Updated Description");
        updated.IsEnabled.Should().BeFalse();

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/type-definitions/{id} returns 409 when duplicate libelle exists")]
    public async Task Patch_ShouldReturn409_WhenDuplicateLibelleExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var duplicateId = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");
        var duplicateTypeDefinition = CreateTestTypeDefinition(duplicateId, "Duplicate Libelle", "Other Description");

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        // Setup duplicate check to return existing TypeDefinition
        _typeDefinitionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicateTypeDefinition);

        var payload = new
        {
            TypeDefinitionId = id,
            Libelle = "Duplicate Libelle" // This libelle already exists
        };

        // Act
        var response = await _client.PatchAsync($"/api/type-definitions/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _typeDefinitionRepoMock.Verify(r => r.Update(It.IsAny<TypeDefinition>()), Times.Never);
    }
}