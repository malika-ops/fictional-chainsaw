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

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.CreateTests;

public class CreateTypeDefinitionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();


    public CreateTypeDefinitionEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TypeDefinition td, CancellationToken _) => td);

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/typedefinitions returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Libelle = "TestType",
            Description = "Test Description",
            IsEnabled = true
        };

        _repoMock
            .Setup(r => r.GetByLibelleAsync(payload.Libelle, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypeDefinition?)null); // Pas de doublon

        // Act
        var response = await _client.PostAsJsonAsync("/api/typedefinitions", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r => r.GetByLibelleAsync(payload.Libelle, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddTypeDefinitionAsync(It.Is<TypeDefinition>(td =>
            td.Libelle == payload.Libelle &&
            td.Description == payload.Description), It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/typedefinitions returns 400 when Libelle already exists")]
    public async Task Post_ShouldReturn400_WhenLibelleAlreadyExists()
    {
        // Arrange
        var payload = new
        {
            Libelle = "ExistingType",
            Description = "Duplicate",
            IsEnabled = true
        };

        var existing = TypeDefinition.Create(TypeDefinitionId.Of(Guid.NewGuid()), payload.Libelle, payload.Description, []);

        _repoMock
            .Setup(r => r.GetByLibelleAsync(payload.Libelle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var response = await _client.PostAsJsonAsync("/api/typedefinitions", payload);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        _repoMock.Verify(r => r.AddTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/typedefinitions returns 400 when Description is missing")]
    public async Task Post_ShouldReturn400_WhenDescriptionIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            Libelle = "No description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/typedefinitions", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("description")[0].GetString()
            .Should().Contain("required");

        _repoMock.Verify(r => r.AddTypeDefinitionAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}