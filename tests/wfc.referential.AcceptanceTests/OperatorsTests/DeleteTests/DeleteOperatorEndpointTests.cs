using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.OperatorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.OperatorsTests.DeleteTests;

public class DeleteOperatorEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "DELETE /api/operators/{id} returns 200 when operator exists")]
    public async Task Delete_ShouldReturn200_WhenOperatorExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "Test Operator");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        // Act
        var response = await _client.DeleteAsync($"/api/operators/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();
        operatorEntity.IsEnabled.Should().BeFalse();
        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/operators/{id} returns 404 when operator is not found")]
    public async Task Delete_ShouldReturn404_WhenOperatorNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Operator?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/operators/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/operators/{id} performs soft delete instead of physical deletion")]
    public async Task Delete_ShouldPerformSoftDelete_InsteadOfPhysicalDeletion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var operatorEntity = CreateTestOperator(id, "OP001", "Active Operator");

        // Verify operator starts as enabled
        operatorEntity.IsEnabled.Should().BeTrue();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorEntity);

        // Act
        var response = await _client.DeleteAsync($"/api/operators/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status changed to inactive (soft delete)
        operatorEntity.IsEnabled.Should().BeFalse();

        // Verify no physical deletion occurred (operator object still exists)
        operatorEntity.Should().NotBeNull();
        operatorEntity.Code.Should().Be("OP001"); // Data still intact
        operatorEntity.LastName.Should().Be("Active Operator");

        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/operators/{id} returns 400 for empty GUID")]
    public async Task Delete_ShouldReturn400_ForEmptyGuid()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/operators/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("OperatorId must be a non-empty GUID");

        _operatorRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<OperatorId>(), It.IsAny<CancellationToken>()), Times.Never);
        _operatorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Operator CreateTestOperator(Guid id, string code, string lastName)
    {
        return Operator.Create(
            OperatorId.Of(id),
            code,
            "ID123456",
            lastName,
            "Test",
            "test@email.com",
            "+212600000000",
            OperatorType.Agence,
            Guid.NewGuid(),
            null);
    }
}
