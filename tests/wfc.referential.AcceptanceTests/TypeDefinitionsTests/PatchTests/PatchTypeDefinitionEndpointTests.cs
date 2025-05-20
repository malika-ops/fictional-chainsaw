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
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.PatchTests;

public class PatchTypeDefinitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();

    public PatchTypeDefinitionEndpointTests(WebApplicationFactory<Program> factory)
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
                    .Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(),
                                                           It.IsAny<CancellationToken>()))
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

    [Fact(DisplayName = "PATCH /api/typedefinitions/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        TypeDefinition? updated = null;
        _repoMock.Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()))
                 .Callback<TypeDefinition, CancellationToken>((td, _) => updated = td)
                 .Returns(Task.CompletedTask);

        var payload = new { TypeDefinitionId = id, Libelle = "New Libelle" };

        var response = await _client.PatchAsync($"/api/typedefinitions/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Libelle.Should().Be("New Libelle");
        updated.Description.Should().Be("Old Description");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "PATCH /api/typedefinitions/{id} returns 200 when updating enabled status")]
    public async Task Patch_ShouldReturn200_WhenUpdatingEnabledStatus()
    {
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Test Libelle", "Test Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        TypeDefinition? updated = null;
        _repoMock.Setup(r => r.UpdateTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()))
                 .Callback<TypeDefinition, CancellationToken>((td, _) => updated = td)
                 .Returns(Task.CompletedTask);

        var payload = new { TypeDefinitionId = id, IsEnabled = false };

        var response = await _client.PatchAsync($"/api/typedefinitions/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();
        updated.Libelle.Should().Be("Test Libelle");
        updated.Description.Should().Be("Test Description");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/typedefinitions/{id} returns 400 when typeDefinition doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenTypeDefinitionDoesNotExist()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((TypeDefinition?)null);

        var payload = new { TypeDefinitionId = id, Libelle = "New Libelle" };

        var response = await _client.PatchAsync($"/api/typedefinitions/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"TypeDefinition with ID {id} not found");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/typedefinitions/{id} returns 400 when Libelle is empty")]
    public async Task Patch_ShouldReturn400_WhenLibelleIsEmpty()
    {
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        var payload = new { TypeDefinitionId = id, Libelle = "" };

        var response = await _client.PatchAsync($"/api/typedefinitions/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("libelle")[0].GetString()
            .Should().Be("Libelle cannot be empty if provided");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/typedefinitions/{id} returns 400 when Description is empty")]
    public async Task Patch_ShouldReturn400_WhenDescriptionIsEmpty()
    {
        var id = Guid.NewGuid();
        var typeDefinition = CreateTestTypeDefinition(id, "Old Libelle", "Old Description");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TypeDefinitionId>(tid => tid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(typeDefinition);

        var payload = new { TypeDefinitionId = id, Description = "" };

        var response = await _client.PatchAsync($"/api/typedefinitions/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("description")[0].GetString()
            .Should().Be("Description cannot be empty if provided");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}