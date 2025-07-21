using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.OperatorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.OperatorsTests.GetByIdTests;

public class GetOperatorByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Operator Make(Guid id, string code = "OP001", string? lastName = null, bool enabled = true)
    {
        var operatorEntity = Operator.Create(
            OperatorId.Of(id),
            code,
            "ID123456",
            lastName ?? $"Operator-{code}",
            "Test",
            $"test-{code}@email.com",
            "+212600000000",
            OperatorType.Agence,
            Guid.NewGuid(),
            null);

        if (!enabled) operatorEntity.Disable();
        return operatorEntity;
    }

    private record OperatorDto(Guid OperatorId, string Code, string IdentityCode, string LastName, string FirstName, string Email, bool IsEnabled, Guid? ProfileId);

    [Fact(DisplayName = "GET /api/operators/{id} → 200 when Operator exists")]
    public async Task Get_ShouldReturn200_WhenOperatorExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "OP001", "Alami");

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/operators/{id}");
        var body = await res.Content.ReadFromJsonAsync<OperatorDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.OperatorId.Should().Be(id);
        body.Code.Should().Be("OP001");
        body.LastName.Should().Be("Alami");
        body.IsEnabled.Should().BeTrue();

        _operatorRepoMock.Verify(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/operators/{id} → 404 when Operator not found")]
    public async Task Get_ShouldReturn404_WhenOperatorNotFound()
    {
        var id = Guid.NewGuid();

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Operator?)null);

        var res = await _client.GetAsync($"/api/operators/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _operatorRepoMock.Verify(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/operators/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/operators/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _operatorRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<OperatorId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/operators/{id} → 200 for disabled Operator")]
    public async Task Get_ShouldReturn200_WhenOperatorDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "OP-DIS", enabled: false);

        _operatorRepoMock.Setup(r => r.GetByIdAsync(OperatorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/operators/{id}");
        var dto = await res.Content.ReadFromJsonAsync<OperatorDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}