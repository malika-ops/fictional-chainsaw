using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.OperatorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.OperatorsTests.CreateTests;

public class CreateOperatorEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private void SetupDefaultMocks()
    {
        // Setup Operator Repository - default behavior for successful scenarios
        _operatorRepoMock.Setup(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Operator o, CancellationToken _) => o);
        _operatorRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        // Setup Agency repository - return valid entities by default
        _agencyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgencyId id, CancellationToken _) => CreateMockAgency(id.Value));
    }

    [Fact(DisplayName = "POST /api/operators returns 200 and Guid when all required fields are provided")]
    public async Task Post_ShouldReturn200_AndId_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        Operator capturedOperator = null;

        SetupDefaultMocks();

        _operatorRepoMock.Setup(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()))
            .Callback<Operator, CancellationToken>((o, _) => capturedOperator = o)
            .ReturnsAsync((Operator o, CancellationToken _) => o);

        var payload = CreateCompleteValidPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().NotBeEmpty();

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Once);
        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedOperator.Should().NotBeNull();
        capturedOperator.Code.Should().Be((string)payload["Code"]);
        capturedOperator.IdentityCode.Should().Be((string)payload["IdentityCode"]);
        capturedOperator.LastName.Should().Be((string)payload["LastName"]);
        capturedOperator.FirstName.Should().Be((string)payload["FirstName"]);
        capturedOperator.Email.Should().Be((string)payload["Email"]);
        capturedOperator.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "POST /api/operators returns 400 when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("Code");

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code is required");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 400 when IdentityCode is missing")]
    public async Task Post_ShouldReturn400_WhenIdentityCodeIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("IdentityCode");

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("IdentityCode is required");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 400 when Email is missing")]
    public async Task Post_ShouldReturn400_WhenEmailIsMissing()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload.Remove("Email");

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Email is required");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 400 when Email format is invalid")]
    public async Task Post_ShouldReturn400_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["Email"] = "invalid-email-format";

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Email must be a valid email address");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenCodeAlreadyExists()
    {
        // Arrange
        const string duplicateCode = "OP001";

        _operatorRepoMock.Reset();
        _agencyRepoMock.Reset();

        var existingOperator = CreateTestOperator(duplicateCode, "ID123", "test@email.com");

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<Operator, bool>> expr, CancellationToken _) =>
            {
                // Check if this is the code check
                if (expr.ToString().Contains("Code"))
                    return new List<Operator> { existingOperator };
                return new List<Operator>();
            });

        _agencyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgencyId id, CancellationToken _) => CreateMockAgency(id.Value));

        var payload = CreateCompleteValidPayload();
        payload["Code"] = duplicateCode;

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Operator with code {duplicateCode} already exists");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 409 when IdentityCode already exists")]
    public async Task Post_ShouldReturn409_WhenIdentityCodeAlreadyExists()
    {
        // Arrange
        const string duplicateIdentityCode = "ID123456";

        _operatorRepoMock.Reset();
        _agencyRepoMock.Reset();

        var existingOperator = CreateTestOperator("OP999", duplicateIdentityCode, "existing@email.com");

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<Operator, bool>> expr, CancellationToken _) =>
            {
                // Check if this is the identity code check
                if (expr.ToString().Contains("IdentityCode"))
                    return new List<Operator> { existingOperator };
                return new List<Operator>();
            });

        _agencyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgencyId id, CancellationToken _) => CreateMockAgency(id.Value));

        var payload = CreateCompleteValidPayload();
        payload["IdentityCode"] = duplicateIdentityCode;

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Operator with identity code {duplicateIdentityCode} already exists");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 409 when Email already exists")]
    public async Task Post_ShouldReturn409_WhenEmailAlreadyExists()
    {
        // Arrange
        const string duplicateEmail = "duplicate@email.com";

        _operatorRepoMock.Reset();
        _agencyRepoMock.Reset();

        var existingOperator = CreateTestOperator("OP999", "ID999", duplicateEmail);

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<System.Func<Operator, bool>> expr, CancellationToken _) =>
            {
                // Check if this is the email check
                if (expr.ToString().Contains("Email"))
                    return new List<Operator> { existingOperator };
                return new List<Operator>();
            });

        _agencyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<AgencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgencyId id, CancellationToken _) => CreateMockAgency(id.Value));

        var payload = CreateCompleteValidPayload();
        payload["Email"] = duplicateEmail;

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Operator with email {duplicateEmail} already exists");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators returns 404 when Branch does not exist")]
    public async Task Post_ShouldReturn404_WhenBranchDoesNotExist()
    {
        // Arrange
        var invalidBranchId = Guid.NewGuid();

        _operatorRepoMock.Reset();
        _agencyRepoMock.Reset();

        _operatorRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Operator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());

        _agencyRepoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(invalidBranchId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agency?)null);

        var payload = CreateCompleteValidPayload();
        payload["BranchId"] = invalidBranchId;

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Branch with ID {invalidBranchId} not found");

        _operatorRepoMock.Verify(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/operators validates field length limits")]
    public async Task Post_ShouldReturn400_WhenFieldsExceedLengthLimits()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["Code"] = new string('X', 51); // Exceeds 50 char limit
        invalidPayload["IdentityCode"] = new string('Y', 51); // Exceeds 50 char limit
        invalidPayload["LastName"] = new string('Z', 101); // Exceeds 100 char limit
        invalidPayload["Email"] = new string('a', 250) + "@test.com"; // Exceeds 255 char limit

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Code cannot exceed 50 characters");
        responseContent.Should().Contain("IdentityCode cannot exceed 50 characters");
        responseContent.Should().Contain("LastName cannot exceed 100 characters");
        responseContent.Should().Contain("Email cannot exceed 255 characters");
    }

    [Fact(DisplayName = "POST /api/operators validates OperatorType enum")]
    public async Task Post_ShouldReturn400_WhenOperatorTypeIsInvalid()
    {
        // Arrange
        var invalidPayload = CreateCompleteValidPayload();
        invalidPayload["OperatorType"] = 999; // Invalid enum value

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OperatorType must be a valid enum value");
    }

    [Fact(DisplayName = "POST /api/operators allows creation with minimal required fields")]
    public async Task Post_ShouldReturn200_WithMinimalRequiredFields()
    {
        // Arrange
        Operator capturedOperator = null;

        SetupDefaultMocks();

        _operatorRepoMock.Setup(r => r.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()))
            .Callback<Operator, CancellationToken>((o, _) => capturedOperator = o)
            .ReturnsAsync((Operator o, CancellationToken _) => o);

        var minimalPayload = CreateMinimalValidPayload();

        // Act
        var response = await _client.PostAsJsonAsync("/api/operators", minimalPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().NotBeEmpty();

        capturedOperator.Should().NotBeNull();
        capturedOperator.Code.Should().Be((string)minimalPayload["Code"]);
        capturedOperator.IdentityCode.Should().Be((string)minimalPayload["IdentityCode"]);
        capturedOperator.Email.Should().Be((string)minimalPayload["Email"]);
    }

    // Helper Methods
    private static Dictionary<string, object> CreateCompleteValidPayload()
    {
        return new Dictionary<string, object>
        {
            { "Code", "OP001" },
            { "IdentityCode", "AB123456" },
            { "LastName", "Alami" },
            { "FirstName", "Ahmed" },
            { "Email", "ahmed.alami@wafacash.com" },
            { "PhoneNumber", "+212612345678" },
            { "OperatorType", (int)OperatorType.Agence },
            { "BranchId", Guid.NewGuid() }
        };
    }

    private static Dictionary<string, object> CreateMinimalValidPayload()
    {
        return new Dictionary<string, object>
        {
            { "Code", "OP001" },
            { "IdentityCode", "AB123456" },
            { "LastName", "Alami" },
            { "FirstName", "Ahmed" },
            { "Email", "ahmed.alami@wafacash.com" },
            { "PhoneNumber", "+212612345678" }
        };
    }

    private static Operator CreateTestOperator(string code, string identityCode, string email)
    {
        return Operator.Create(
            OperatorId.Of(Guid.NewGuid()),
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