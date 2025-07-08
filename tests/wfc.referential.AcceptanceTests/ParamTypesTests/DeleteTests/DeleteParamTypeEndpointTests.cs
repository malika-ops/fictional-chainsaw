using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.DeleteTests;

public class DeleteParamTypeEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    // Helper to build test ParamType
    private static ParamType CreateTestParamType(Guid id, string value)
    {
        var typeDefinitionId = TypeDefinitionId.Of(Guid.NewGuid());
        return ParamType.Create(
            new ParamTypeId(id),
            typeDefinitionId,
            value
        );
    }

    [Fact(DisplayName = "DELETE /api/paramtypes/{id} returns 200 when paramtype exists")]
    public async Task Delete_ShouldReturn200_WhenParamTypeExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Test Value");

        _paramTypeRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paramType);

        // Act
        var response = await _client.DeleteAsync($"/api/paramtypes/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        // Verify status changed to inactive (soft delete) - the domain entity itself is modified
        paramType.IsEnabled.Should().BeFalse();

        // Verify repository interactions - Delete handler calls Disable() and SaveChangesAsync()
        _paramTypeRepoMock.Verify(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()), Times.Once);
        _paramTypeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/paramtypes/{id} returns 400 when paramtype is not found")]
    public async Task Delete_ShouldReturn400_WhenParamTypeNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _paramTypeRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/paramtypes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Matches Bank pattern

        _paramTypeRepoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
        _paramTypeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/paramtypes/{id} changes status to inactive instead of physical deletion")]
    public async Task Delete_ShouldChangeStatusToInactive_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Test Value");

        // Verify paramType starts as enabled
        paramType.IsEnabled.Should().BeTrue();

        _paramTypeRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paramType);

        // Act
        var response = await _client.DeleteAsync($"/api/paramtypes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        paramType.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (paramType object still exists)
        paramType.Should().NotBeNull();
        paramType.Value.Should().Be("Test Value"); // Data still intact
    }

    [Fact(DisplayName = "DELETE /api/paramtypes/{id} validates paramType exists before deletion")]
    public async Task Delete_ShouldValidateParamTypeExists_BeforeDeletion()
    {
        // Arrange
        var nonExistentParamTypeId = Guid.NewGuid();

        _paramTypeRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == nonExistentParamTypeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/paramtypes/{nonExistentParamTypeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no save operation was attempted
        _paramTypeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/paramtypes/{id} returns 400 for invalid GUID format")]
    public async Task Delete_ShouldReturnBadRequest_ForInvalidGuidFormat()
    {
        // Act
        var response = await _client.DeleteAsync("/api/paramtypes/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no repository operations were attempted
        _paramTypeRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()), Times.Never);
        _paramTypeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}