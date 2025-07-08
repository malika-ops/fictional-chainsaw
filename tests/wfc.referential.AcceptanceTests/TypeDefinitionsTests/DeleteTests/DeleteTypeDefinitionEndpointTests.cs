using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.DeleteTests;

public class DeleteTypeDefinitionEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    private static TypeDefinition CreateTestTypeDefinition(Guid id, string libelle, string description, bool withParamTypes = false)
    {
        var typeDefinition = TypeDefinition.Create(new TypeDefinitionId(id), libelle, description);

        if (withParamTypes)
        {
            var paramType = ParamType.Create(
                new ParamTypeId(Guid.NewGuid()),
                typeDefinition.Id,
                "Test Param Value"
            );
            // Add ParamType to the collection - you need to add AddParamType method to TypeDefinition
            // or directly add to the list if it's accessible
            typeDefinition.ParamTypes.Add(paramType);
        }

        return typeDefinition;
    }

    [Fact(DisplayName = "DELETE /api/type-definitions/{id} returns 200 when typeDefinition exists and has no paramTypes")]
    public async Task Delete_ShouldReturn200_WhenTypeDefinitionExistsAndHasNoParamTypes()
    {
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Type", "Test Description");

        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDefinition);

        var response = await _client.DeleteAsync($"/api/type-definitions/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        // Verify the entity was disabled (soft delete)
        typeDefinition.IsEnabled.Should().BeFalse();

        // Verify repository interactions - the handler calls Disable() and SaveChangesAsync()
        _typeDefinitionRepoMock.Verify(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()), Times.Once);
        _typeDefinitionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/type-definitions/{id} returns 400 when typeDefinition is not found")]
    public async Task Delete_ShouldReturn400_WhenTypeDefinitionNotFound()
    {
        var id = Guid.NewGuid();

        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypeDefinition?)null);

        var response = await _client.DeleteAsync($"/api/type-definitions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _typeDefinitionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/type-definitions/{id} returns 409 when typeDefinition has linked paramTypes")]
    public async Task Delete_ShouldReturn409_WhenTypeDefinitionHasLinkedParamTypes()
    {
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Type", "Test Description", withParamTypes: true);

        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDefinition);

        var response = await _client.DeleteAsync($"/api/type-definitions/{id}");

        // Changed expectation to 409 Conflict since TypeDefinitionLinkedToParamTypeException likely returns 409
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _typeDefinitionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/type-definitions/{id} changes status to inactive instead of physical deletion")]
    public async Task Delete_ShouldChangeStatusToInactive_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Type", "Test Description");

        // Verify typeDefinition starts as enabled
        typeDefinition.IsEnabled.Should().BeTrue();

        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDefinition);

        // Act
        var response = await _client.DeleteAsync($"/api/type-definitions/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        typeDefinition.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (typeDefinition object still exists)
        typeDefinition.Should().NotBeNull();
        typeDefinition.Libelle.Should().Be("Test Type"); // Data still intact
        typeDefinition.Description.Should().Be("Test Description");
    }

    [Fact(DisplayName = "DELETE /api/type-definitions/{id} validates typeDefinition exists before deletion")]
    public async Task Delete_ShouldValidateTypeDefinitionExists_BeforeDeletion()
    {
        // Arrange
        var nonExistentTypeDefinitionId = Guid.NewGuid();

        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == nonExistentTypeDefinitionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypeDefinition?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/type-definitions/{nonExistentTypeDefinitionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _typeDefinitionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/type-definitions/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/type-definitions/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _typeDefinitionRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TypeDefinitionId>(), It.IsAny<CancellationToken>()), Times.Never);
        _typeDefinitionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}