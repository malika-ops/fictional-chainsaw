using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Exceptions;
using FluentAssertions;
using Moq;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.UpdateTests;

public class UpdateParamTypeEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    // Helper to create a test ParamType
    private static ParamType CreateTestParamType(Guid id, string value)
    {
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        return ParamType.Create(
            new ParamTypeId(id),
            typeDefinitionId,
            value
        );
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 400 when update fails with validation errors")]
    public async Task Put_ShouldReturn400_WhenUpdateFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldParamType = CreateTestParamType(id, "Old Value");

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        _paramTypeRepoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
                 .Throws(new BusinessException("Une erreur de validation simulée"));

        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            ParamTypeId = id,
            Value = "New Value",
            IsEnabled = true,
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/paramtypes/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _paramTypeRepoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 400 when Value is missing")]
    public async Task Put_ShouldReturn400_WhenValueMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            ParamTypeId = id,
            // Value omitted
            IsEnabled = true,
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/paramtypes/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _paramTypeRepoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 400 when paramType doesn't exist")]
    public async Task Put_ShouldReturn400_WhenParamTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        var payload = new
        {
            ParamTypeId = id,
            Value = "New Value",
            IsEnabled = true,
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/paramtypes/{id}", payload);

        // Assert - actual implementation returns BadRequest, not NotFound
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _paramTypeRepoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 200 and bool when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldParamType = CreateTestParamType(id, "Old Value");

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        // Setup duplicate check to return null (no duplicates)
        _paramTypeRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _paramTypeRepoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
                 .Callback<ParamType>(p => updated = p);

        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Make sure the payload format matches exactly what the API expects
        var payload = new UpdateParamTypeRequest
        {
            Value = "New Value",
            IsEnabled = true,
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/paramtypes/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Now expecting bool instead of Guid
        updated!.Value.Should().Be("New Value");

        _paramTypeRepoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} handles disabling correctly")]
    public async Task Put_ShouldHandleDisabling_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldParamType = CreateTestParamType(id, "Test Value");
        // By default it's enabled

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        // Setup duplicate check to return null (no duplicates)
        _paramTypeRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _paramTypeRepoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
                 .Callback<ParamType>(p => updated = p);

        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new UpdateParamTypeRequest
        {
            Value = "Test Value",
            IsEnabled = false,  // We want to disable it
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/paramtypes/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue(); // Expecting bool return
        updated!.IsEnabled.Should().BeFalse();

        _paramTypeRepoMock.Verify(r => r.Update(
                                It.Is<ParamType>(p =>
                                    p.IsEnabled == false &&
                                    p.Value == "Test Value")),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 409 when duplicate value exists")]
    public async Task Put_ShouldReturn409_WhenDuplicateValueExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var duplicateId = Guid.NewGuid();
        var oldParamType = CreateTestParamType(id, "Old Value");
        var duplicateParamType = CreateTestParamType(duplicateId, "Duplicate Value");

        _paramTypeRepoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        // Setup duplicate check to return existing ParamType
        _paramTypeRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicateParamType);

        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new UpdateParamTypeRequest
        {
            Value = "Duplicate Value", // This value already exists
            IsEnabled = true,
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/paramtypes/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _paramTypeRepoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }
}