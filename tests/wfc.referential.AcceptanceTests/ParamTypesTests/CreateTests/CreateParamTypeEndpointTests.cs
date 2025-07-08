using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.CreateTests;

public class CreateParamTypeEndpointTests : BaseAcceptanceTests
{
    public CreateParamTypeEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {
        // Setup duplicate check to return null by default (no duplicates)
        _paramTypeRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParamType?)null);

        // Set up typeDefinition mock to return valid entities
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TypeDefinitionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TypeDefinition.Create(typeDefinitionId, "TestType", "Test Type Definition"));

    }

    [Fact(DisplayName = "POST /api/paramtypes returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            Value = "TestValue",
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _paramTypeRepoMock.Verify(r =>
            r.AddAsync(It.Is<ParamType>(p =>
                    p.Value == payload.Value &&
                    p.IsEnabled == true), // Default value
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/paramtypes returns 400 & problem-details when Value is missing")]
    public async Task Post_ShouldReturn400_WhenValueIsMissing()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var invalidPayload = new
        {
            // Value intentionally omitted to trigger validation error
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The handler must NOT be reached
        _paramTypeRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/paramtypes returns 400 when TypeDefinition is not found")]
    public async Task Post_ShouldReturn400_WhenTypeDefinitionNotFound()
    {
        // Arrange
        var nonExistentTypeDefinitionId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Setup repository to return null for this ID
        _typeDefinitionRepoMock
            .Setup(r => r.GetByIdAsync(TypeDefinitionId.Of(nonExistentTypeDefinitionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TypeDefinition?)null);

        var payload = new
        {
            Value = "TestValue",
            TypeDefinitionId = nonExistentTypeDefinitionId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Handler must NOT attempt to add the entity
        _paramTypeRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/paramtypes returns 409 when duplicate value exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateValueExists()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var existingParamType = ParamType.Create(
            ParamTypeId.Of(Guid.NewGuid()),
            TypeDefinitionId.Of(typeDefinitionId),
            "DuplicateValue");

        // Setup duplicate check to return existing ParamType
        _paramTypeRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ParamType, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingParamType);

        var payload = new
        {
            Value = "DuplicateValue",
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Handler must NOT attempt to add the entity
        _paramTypeRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/paramtypes auto-generates paramType ID")]
    public async Task Post_ShouldAutoGenerateParamTypeId_WhenParamTypeIsCreated()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            Value = "TestValue",
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);
        var paramTypeId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paramTypeId.Should().NotBeEmpty();

        _paramTypeRepoMock.Verify(r => r.AddAsync(It.Is<ParamType>(p =>
            p.Id != null && p.Id.Value != Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/paramtypes sets IsEnabled to true by default")]
    public async Task Post_ShouldSetIsEnabledToTrue_ByDefault()
    {
        // Arrange
        var typeDefinitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            Value = "TestValue",
            TypeDefinitionId = typeDefinitionId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _paramTypeRepoMock.Verify(r => r.AddAsync(It.Is<ParamType>(p =>
            p.IsEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "POST /api/paramtypes validates required fields")]
    [InlineData("", "22222222-2222-2222-2222-222222222222")]
    [InlineData("TestValue", "00000000-0000-0000-0000-000000000000")]
    public async Task Post_ShouldReturnValidationError_WhenRequiredFieldsAreMissing(
        string value, string typeDefinitionId)
    {
        // Arrange
        var payload = new
        {
            Value = value,
            TypeDefinitionId = Guid.Parse(typeDefinitionId)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/paramtypes", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _paramTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<ParamType>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}