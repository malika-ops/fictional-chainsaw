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
                    .Setup(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                      It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

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

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        ParamType? updated = null;
        _repoMock.Setup(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                   It.IsAny<CancellationToken>()))
                 .Callback<ParamType, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            ParamTypeId = id,
            Value = "New Value",
            TypeDefinitionId = typeDefinitionId
            // IsEnabled intentionally omitted - should not change
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Value.Should().Be("New Value");
        updated.IsEnabled.Should().BeTrue();  // Enabled status should not change

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 200 when updating enabled status")]
    public async Task Patch_ShouldReturn200_WhenUpdatingEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Test Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        ParamType? updated = null;
        _repoMock.Setup(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                   It.IsAny<CancellationToken>()))
                 .Callback<ParamType, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            ParamTypeId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();
        updated.Value.Should().Be("Test Value");  // Value should not change

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 400 when paramType doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenParamTypeDoesNotExist()
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
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"ParamType with ID {id} not found");

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} returns 400 when Value is empty")]
    public async Task Patch_ShouldReturn400_WhenValueIsEmpty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var paramType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        var payload = new
        {
            ParamTypeId = id,
            Value = "",  // Empty value
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("value")[0].GetString()
            .Should().Be("Value cannot be empty if provided");

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/paramtypes/{id} handles both value and IsEnabled changes")]
    public async Task Patch_ShouldHandleBothValueAndIsEnabledChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paramType = CreateTestParamType(id, "Old Value");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(paramType);

        ParamType? updated = null;
        _repoMock.Setup(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                   It.IsAny<CancellationToken>()))
                 .Callback<ParamType, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            ParamTypeId = id,
            Value = "Updated Value",
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsync($"/api/paramtypes/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Value.Should().Be("Updated Value");
        updated.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                          Times.Once);
    }
}