using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.UpdateTests;

public class UpdateParamTypeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repoMock = new();
    private readonly Mock<ITypeDefinitionRepository> _typeDefinitionRepoMock = new();

    public UpdateParamTypeEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.Update(It.IsAny<ParamType>()));

                // Set up typeDefinition mock to return valid entities
                var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));

                _typeDefinitionRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<TypeDefinitionId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(TypeDefinition.Create(typeDefinitionId, "TestType", "Test Type Definition"));

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_typeDefinitionRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

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

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        _repoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
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

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
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

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 400 when paramType doesn't exist")]
    public async Task Put_ShouldReturn400_WhenParamTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
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

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} returns 200 and bool when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldParamType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
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

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/paramtypes/{id} handles disabling correctly")]
    public async Task Put_ShouldHandleDisabling_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldParamType = CreateTestParamType(id, "Test Value");
        // By default it's enabled

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
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

        _repoMock.Verify(r => r.Update(
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

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldParamType);

        // Setup duplicate check to return existing ParamType
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
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

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }
}