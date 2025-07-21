using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.OperatorsTests.PatchTests;

public class PatchOperatorEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PATCH /api/operators/{id} returns 200 when patching single field")]
    public async Task Patch_ShouldReturn200_WhenPatchingSingleField()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "old@email.com");
        var originalPhone = operatorEntity.PhoneNumber;

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        var patchPayload = new { OperatorId = id, LastName = "Updated LastName" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        operatorEntity.LastName.Should().Be("Updated LastName");
        operatorEntity.Code.Should().Be("OP001"); // Should remain unchanged
        operatorEntity.PhoneNumber.Should().Be(originalPhone); // Should remain unchanged

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} returns 200 when patching multiple fields")]
    public async Task Patch_ShouldReturn200_WhenPatchingMultipleFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "old@email.com");
        var originalCode = operatorEntity.Code;

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        var patchPayload = new
        {
            OperatorId = id,
            LastName = "Updated LastName",
            FirstName = "Updated FirstName",
            IsEnabled = false
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify specified fields were updated
        operatorEntity.LastName.Should().Be("Updated LastName");
        operatorEntity.FirstName.Should().Be("Updated FirstName");
        operatorEntity.IsEnabled.Should().BeFalse();

        // Verify unspecified fields remained unchanged
        operatorEntity.Code.Should().Be(originalCode);

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} returns 404 when operator not found")]
    public async Task Patch_ShouldReturn404_WhenOperatorNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Operator?)null);

        var patchPayload = new { OperatorId = id, LastName = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Operator with ID {id} was not found");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} returns 409 when code already exists")]
    public async Task Patch_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetOperator = CreateTestOperator(id, "OP001", "ID001", "test1@email.com");
        var conflictingOperator = CreateTestOperator(existingId, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator> { conflictingOperator });

        var patchPayload = new { OperatorId = id, Code = "OP002" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OP002 already exists");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} returns 409 when identity code already exists")]
    public async Task Patch_ShouldReturn409_WhenIdentityCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetOperator = CreateTestOperator(id, "OP001", "ID001", "test1@email.com");
        var conflictingOperator = CreateTestOperator(existingId, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator> { conflictingOperator });

        var patchPayload = new { OperatorId = id, IdentityCode = "ID002" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ID002 already exists");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} returns 409 when email already exists")]
    public async Task Patch_ShouldReturn409_WhenEmailAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetOperator = CreateTestOperator(id, "OP001", "ID001", "test1@email.com");
        var conflictingOperator = CreateTestOperator(existingId, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator> { conflictingOperator });

        var patchPayload = new { OperatorId = id, Email = "test2@email.com" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("test2@email.com already exists");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} validates Branch exists when provided")]
    public async Task Patch_ShouldReturn404_WhenBranchDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");
        var invalidBranchId = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(invalidBranchId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agency?)null);

        var patchPayload = new { OperatorId = id, BranchId = invalidBranchId };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Branch with ID {invalidBranchId} not found");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} returns 400 for empty GUID")]
    public async Task Patch_ShouldReturn400_ForEmptyGuid()
    {
        // Arrange
        var patchPayload = new { OperatorId = Guid.Empty, LastName = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{Guid.Empty}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OperatorId cannot be empty");

        _operatorRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<OperatorId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} validates field length limits")]
    public async Task Patch_ShouldReturn400_WhenFieldsExceedLengthLimits()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var patchPayload = new
        {
            OperatorId = id,
            Code = new string('X', 51), // Exceeds 50 char limit
            IdentityCode = new string('Y', 51), // Exceeds 50 char limit
            LastName = new string('Z', 101), // Exceeds 100 char limit
            FirstName = new string('A', 101), // Exceeds 100 char limit
            Email = new string('B', 250) + "@test.com" // Exceeds 255 char limit
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot exceed 50 characters");
        responseContent.Should().Contain("IdentityCode cannot exceed 50 characters");
        responseContent.Should().Contain("LastName cannot exceed 100 characters");
        responseContent.Should().Contain("FirstName cannot exceed 100 characters");
        responseContent.Should().Contain("Email cannot exceed 255 characters");
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} validates email format when provided")]
    public async Task Patch_ShouldReturn400_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var patchPayload = new
        {
            OperatorId = id,
            Email = "invalid-email-format"
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Email must be a valid email address");
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} validates OperatorType enum when provided")]
    public async Task Patch_ShouldReturn400_WhenOperatorTypeIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var patchPayload = new
        {
            OperatorId = id,
            OperatorType = 999 // Invalid enum value
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OperatorType must be a valid enum value");
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} allows empty patch (no fields to update)")]
    public async Task Patch_ShouldReturn200_WhenNoFieldsToUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var patchPayload = new { OperatorId = id }; // Only ID, no fields to update

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} handles null values correctly")]
    public async Task Patch_ShouldReturn400_WhenProvidingEmptyStrings()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var patchPayload = new
        {
            OperatorId = id,
            Code = "", // Empty string should fail validation
            IdentityCode = "", // Empty string should fail validation
            LastName = "", // Empty string should fail validation
            Email = "" // Empty string should fail validation
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot be empty if provided");
        responseContent.Should().Contain("IdentityCode cannot be empty if provided");
        responseContent.Should().Contain("LastName cannot be empty if provided");
        responseContent.Should().Contain("Email cannot be empty if provided");
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} allows partial update with valid Branch")]
    public async Task Patch_ShouldReturn200_WhenUpdatingWithValidBranch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(branchId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockAgency());

        var patchPayload = new
        {
            OperatorId = id,
            BranchId = branchId,
            OperatorType = (int)OperatorType.Filiale
        };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        _agencyRepoMock.Verify(r => r.GetByIdAsync(AgencyId.Of(branchId), It.IsAny<CancellationToken>()), Times.Once);
        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} handles concurrent patch requests")]
    public async Task Patch_ShouldHandleConcurrentPatchRequests()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var operator1 = CreateTestOperator(id1, "OP001", "ID001", "test1@email.com");
        var operator2 = CreateTestOperator(id2, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operator1);
        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operator2);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        var patchPayload1 = new { OperatorId = id1, LastName = "Updated Operator 1" };
        var patchPayload2 = new { OperatorId = id2, LastName = "Updated Operator 2" };

        // Act - Simulate concurrent requests
        var tasks = new[]
        {
            _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id1}")
            {
                Content = JsonContent.Create(patchPayload1)
            }),
            _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{id2}")
            {
                Content = JsonContent.Create(patchPayload2)
            })
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeTrue();
        }

        // Verify both operators were updated
        operator1.LastName.Should().Be("Updated Operator 1");
        operator2.LastName.Should().Be("Updated Operator 2");

        _operatorRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<OperatorId>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "PATCH /api/operators/{id} validates URL parameter matches request body")]
    public async Task Patch_ShouldValidateUrlParameterMatchesRequestBody()
    {
        // Arrange
        var urlId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(urlId, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(urlId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var patchPayload = new { OperatorId = bodyId, LastName = "Updated Name" };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"/api/operators/{urlId}")
        {
            Content = JsonContent.Create(patchPayload)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the URL ID was used (not the body ID)
        _operatorRepoMock.Verify(r => r.GetByIdAsync(OperatorId.Of(urlId), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper methods
    private static Operator CreateTestOperator(Guid id, string code, string identityCode, string email)
    {
        return Operator.Create(
            OperatorId.Of(id),
            code,
            identityCode,
            "Test",
            "Operator",
            email,
            "+212600000000",
            OperatorType.Agence,
            Guid.NewGuid());
    }

    private static Agency CreateMockAgency()
    {
        return Agency.Create(
            AgencyId.Of(Guid.NewGuid()),
            "AG001",
            "Test Agency",
            "TA",
            "Test Address",
            null,
            "+212500000000",
            "+212500000001",
            "Test Sheet",
            "ACC001",
            "12345",
            null, null, null, null, null, null, null,
            CityId.Of(Guid.NewGuid()),
            null, null, null, null, null, null);
    }
}