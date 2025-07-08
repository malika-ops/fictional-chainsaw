using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Constants;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.CreateTests;

public class CreateTypeDefinitionEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "POST /api/type-definitions returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var payload = new
        {
            Libelle = "TestType",
            Description = "Test Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedId.Should().NotBeEmpty();

        _typeDefinitionRepoMock.Verify(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.Is<TypeDefinition>(td =>
            td.Libelle == payload.Libelle &&
            td.Description == payload.Description), It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(c => c.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/type-definitions returns 409 when Libelle already exists")]
    public async Task Post_ShouldReturn409_WhenLibelleAlreadyExists()
    {
        // Arrange
        var payload = new
        {
            Libelle = "ExistingType",
            Description = "Duplicate",
            IsEnabled = true
        };

        // Fix: Remove the 4th parameter (empty array) - Create only takes 3 parameters
        var existing = TypeDefinition.Create(
            TypeDefinitionId.Of(Guid.NewGuid()),
            payload.Libelle,
            payload.Description);

        _typeDefinitionRepoMock
            .Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TypeDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/type-definitions returns 400 when Description is missing")]
    public async Task Post_ShouldReturn400_WhenDescriptionIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            Libelle = "No description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/type-definitions returns 400 when Libelle is missing")]
    public async Task Post_ShouldReturn400_WhenLibelleIsMissing()
    {
        // Arrange
        var invalidPayload = new
        {
            Description = "Test Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/type-definitions auto-generates typeDefinition ID")]
    public async Task Post_ShouldAutoGenerateTypeDefinitionId_WhenTypeDefinitionIsCreated()
    {
        // Arrange
        var payload = new
        {
            Libelle = "TestType",
            Description = "Test Description",
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", payload);
        var typeDefinitionId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        typeDefinitionId.Should().NotBeEmpty();

        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.Is<TypeDefinition>(td =>
            td.Id != null && td.Id.Value != Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/type-definitions sets IsEnabled to true by default")]
    public async Task Post_ShouldSetIsEnabledToTrue_ByDefault()
    {
        // Arrange
        var payload = new
        {
            Libelle = "TestType",
            Description = "Test Description"
            // IsEnabled intentionally omitted
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.Is<TypeDefinition>(td =>
            td.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "POST /api/type-definitions validates required fields")]
    [InlineData("", "Test Description")]
    [InlineData("TestType", "")]
    public async Task Post_ShouldReturnValidationError_WhenRequiredFieldsAreMissing(
        string libelle, string description)
    {
        // Arrange
        var payload = new
        {
            Libelle = libelle,
            Description = description
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/type-definitions", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _typeDefinitionRepoMock.Verify(r => r.AddAsync(It.IsAny<TypeDefinition>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}