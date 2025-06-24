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

namespace wfc.referential.AcceptanceTests.ParamTypesTests.PatchTests;

public class PatchParamTypeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repoMock = new();
    private readonly Mock<ITypeDefinitionRepository> _typeDefinitionRepoMock = new();

    public PatchParamTypeEndpointTests(WebApplicationFactory<Program> factory)
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

                // Set up typeDefinition mock
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

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 200 and bool when patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
                 .Callback<ParamType>(p => updated = p);

        var payload = new
        {
            ParamTypeId = id,
            Value = "New Value"
            // IsEnabled intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.Value.Should().Be("New Value");
        updated.IsEnabled.Should().BeTrue();  // Enabled status should not change

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 200 when updating enabled status")]
    public async Task Patch_ShouldReturn200_WhenUpdatingEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Test Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
                 .Callback<ParamType>(p => updated = p);

        var payload = new
        {
            ParamTypeId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.IsEnabled.Should().BeFalse();
        updated.Value.Should().Be("Test Value");  // Value should not change

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 404 when paramType doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenParamTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        var payload = new
        {
            ParamTypeId = id,
            Value = "New Value"
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Updated to expect 404 like Bank pattern

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 400 when Value is empty")]
    public async Task Patch_ShouldReturn400_WhenValueIsEmpty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        var payload = new
        {
            ParamTypeId = id,
            Value = ""  // Empty value
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} handles both value and IsEnabled changes")]
    public async Task Patch_ShouldHandleBothValueAndIsEnabledChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        // Setup duplicate check to return null (no duplicates)
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ParamType?)null);

        ParamType? updated = null;
        _repoMock.Setup(r => r.Update(It.IsAny<ParamType>()))
                 .Callback<ParamType>(p => updated = p);

        var payload = new
        {
            ParamTypeId = id,
            Value = "Updated Value",
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue(); // Now expecting bool instead of Guid

        updated!.Value.Should().Be("Updated Value");
        updated.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 409 when duplicate value exists")]
    public async Task Patch_ShouldReturn409_WhenDuplicateValueExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var duplicateId = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Old Value");
        var duplicateParamType = CreateTestParamType(duplicateId, "Duplicate Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        // Setup duplicate check to return existing ParamType
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicateParamType);

        var payload = new
        {
            ParamTypeId = id,
            Value = "Duplicate Value" // This value already exists
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r => r.Update(It.IsAny<ParamType>()), Times.Never);
    }
}