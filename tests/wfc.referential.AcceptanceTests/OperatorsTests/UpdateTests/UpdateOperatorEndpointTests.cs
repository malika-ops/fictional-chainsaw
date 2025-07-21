using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.OperatorsTests.UpdateTests;

public class UpdateOperatorEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PUT /api/operators/{id} returns 200 when update succeeds with all fields")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessfulWithAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldOperator = CreateTestOperator(id, "OP001", "ID123", "old@email.com");
        var branchId = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(branchId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockAgency(branchId));

        Operator updated = null;
        _operatorRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => updated = oldOperator)
            .Returns(Task.CompletedTask);

        var payload = CreateCompleteUpdatePayload(id, branchId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        updated.Code.Should().Be("OP002");
        updated.IdentityCode.Should().Be("CD789012");
        updated.LastName.Should().Be("Benali");
        updated.FirstName.Should().Be("Fatima");
        updated.Email.Should().Be("fatima.benali@wafacash.com");
        updated.IsEnabled.Should().BeTrue();

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} returns 404 when operator doesn't exist")]
    public async Task Put_ShouldReturn404_WhenOperatorDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Operator?)null);

        var payload = CreateBasicUpdatePayload(id);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Operator with ID {id} was not found");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} validates Branch exists when provided")]
    public async Task Put_ShouldReturn404_WhenBranchDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");
        var invalidBranchId = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(invalidBranchId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agency?)null);

        var payload = CreateBasicUpdatePayloadWithBranch(id, invalidBranchId);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Branch with ID {invalidBranchId} not found");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            OperatorId = id,
            // Code intentionally omitted
            IdentityCode = "ID123456",
            LastName = "Test",
            FirstName = "Operator",
            Email = "test@email.com",
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} returns 400 when Email is missing")]
    public async Task Put_ShouldReturn400_WhenEmailMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        var payload = new
        {
            OperatorId = id,
            Code = "OP001",
            IdentityCode = "ID123456",
            LastName = "Test",
            FirstName = "Operator",
            // Email intentionally omitted
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Email is required");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} validates field length limits")]
    public async Task Put_ShouldReturn400_WhenFieldsExceedLengthLimits()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var payload = new
        {
            OperatorId = id,
            Code = new string('X', 51), // Exceeds 50 char limit
            IdentityCode = new string('Y', 51), // Exceeds 50 char limit
            LastName = new string('Z', 101), // Exceeds 100 char limit
            FirstName = new string('A', 101), // Exceeds 100 char limit
            Email = new string('B', 250) + "@test.com", // Exceeds 255 char limit
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot exceed 50 characters");
        responseContent.Should().Contain("IdentityCode cannot exceed 50 characters");
        responseContent.Should().Contain("LastName cannot exceed 100 characters");
        responseContent.Should().Contain("FirstName cannot exceed 100 characters");
        responseContent.Should().Contain("Email cannot exceed 255 characters");
    }

    [Fact(DisplayName = "PUT /api/operators/{id} validates email format")]
    public async Task Put_ShouldReturn400_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        var payload = new
        {
            OperatorId = id,
            Code = "OP001",
            IdentityCode = "ID123456",
            LastName = "Test",
            FirstName = "Operator",
            Email = "invalid-email-format", // Invalid email format
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Email must be a valid email address");
    }
    [Fact(DisplayName = "PUT /api/operators/{id} returns 409 when code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetOperator = CreateTestOperator(id, "OP001", "ID001", "test1@email.com");
        var conflictingOperator = CreateTestOperator(existingId, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<Operator, bool>> expr, CancellationToken ct) =>
            {
                var expressionString = expr.ToString();

                // Return conflicting operator only for code check
                if (expressionString.Contains("Code") && !expressionString.Contains("IdentityCode"))
                    return new List<Operator> { conflictingOperator };

                return new List<Operator>();
            });

        var payload = CreateBasicUpdatePayloadWithCode(id, "OP002");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OP002 already exists");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} returns 409 when identity code already exists")]
    public async Task Put_ShouldReturn409_WhenIdentityCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetOperator = CreateTestOperator(id, "OP001", "ID001", "test1@email.com");
        var conflictingOperator = CreateTestOperator(existingId, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<Operator, bool>> expr, CancellationToken ct) =>
            {
                var expressionString = expr.ToString();

                // Return conflicting operator only for identity code check
                if (expressionString.Contains("IdentityCode"))
                    return new List<Operator> { conflictingOperator };

                return new List<Operator>();
            });

        var payload = CreateBasicUpdatePayloadWithIdentityCode(id, "ID002");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("ID002 already exists");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/operators/{id} returns 409 when email already exists")]
    public async Task Put_ShouldReturn409_WhenEmailAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var targetOperator = CreateTestOperator(id, "OP001", "ID001", "test1@email.com");
        var conflictingOperator = CreateTestOperator(existingId, "OP002", "ID002", "test2@email.com");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetOperator);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<Operator, bool>> expr, CancellationToken ct) =>
            {
                var expressionString = expr.ToString();

                // Return conflicting operator only for email check
                if (expressionString.Contains("Email"))
                    return new List<Operator> { conflictingOperator };

                return new List<Operator>();
            });

        var payload = CreateBasicUpdatePayloadWithEmail(id, "test2@email.com");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/operators/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("test2@email.com already exists");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper methods
    private static object CreateCompleteUpdatePayload(Guid id, Guid branchId)
    {
        return new
        {
            OperatorId = id,
            Code = "OP002",
            IdentityCode = "CD789012",
            LastName = "Benali",
            FirstName = "Fatima",
            Email = "fatima.benali@wafacash.com",
            PhoneNumber = "+212698765432",
            IsEnabled = true,
            OperatorType = (int)OperatorType.Filiale,
            BranchId = branchId
        };
    }

    private static object CreateBasicUpdatePayload(Guid id)
    {
        return new
        {
            OperatorId = id,
            Code = "OP001",
            IdentityCode = "AB123456",
            LastName = "Test",
            FirstName = "Operator",
            Email = "test@email.com",
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };
    }

    private static object CreateBasicUpdatePayloadWithCode(Guid id, string code)
    {
        return new
        {
            OperatorId = id,
            Code = code,
            IdentityCode = "AB123456",
            LastName = "Test",
            FirstName = "Operator",
            Email = "test@email.com",
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };
    }

    private static object CreateBasicUpdatePayloadWithIdentityCode(Guid id, string identityCode)
    {
        return new
        {
            OperatorId = id,
            Code = "OP001",
            IdentityCode = identityCode,
            LastName = "Test",
            FirstName = "Operator",
            Email = "test@email.com",
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };
    }

    private static object CreateBasicUpdatePayloadWithEmail(Guid id, string email)
    {
        return new
        {
            OperatorId = id,
            Code = "OP001",
            IdentityCode = "AB123456",
            LastName = "Test",
            FirstName = "Operator",
            Email = email,
            PhoneNumber = "+212600000000",
            IsEnabled = true
        };
    }

    private static object CreateBasicUpdatePayloadWithBranch(Guid id, Guid branchId)
    {
        return new
        {
            OperatorId = id,
            Code = "OP001",
            IdentityCode = "AB123456",
            LastName = "Test",
            FirstName = "Operator",
            Email = "test@email.com",
            PhoneNumber = "+212600000000",
            IsEnabled = true,
            BranchId = branchId
        };
    }

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

    private static Agency CreateMockAgency(Guid? id = null)
    {
        return Agency.Create(
            AgencyId.Of(id ?? Guid.NewGuid()),
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