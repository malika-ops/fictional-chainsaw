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

namespace wfc.referential.AcceptanceTests.ParamTypesTests.DeleteTests;

public class DeleteParamTypeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repoMock = new();

    public DeleteParamTypeEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

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

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paramType);

        // Capture the entity passed to Update
        ParamType? updatedParamType = null;
        _repoMock
            .Setup(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()))
            .Callback<ParamType, CancellationToken>((p, _) => updatedParamType = p)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/paramtypes/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedParamType!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                                                    Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/paramtypes/{id} returns 404 when paramtype is not found")]
    public async Task Delete_ShouldReturn404_WhenParamTypeNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<ParamTypeId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/paramtypes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.UpdateParamTypeAsync(It.IsAny<ParamType>(),
                                                    It.IsAny<CancellationToken>()),
                                                    Times.Never);
    }
}