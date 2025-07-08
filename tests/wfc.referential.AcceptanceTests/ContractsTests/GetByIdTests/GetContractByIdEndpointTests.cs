using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PartnerAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractsTests.GetByIdTests;

public class GetContractByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Contract Make(Guid id, string code = "CONTRACT-001", string? name = null, bool enabled = true)
    {
        var contract = Contract.Create(
            id: ContractId.Of(id),
            code: code,
            partnerId: PartnerId.Of(Guid.NewGuid()), // Default partner ID
            startDate: DateTime.UtcNow.Date, // Default start date (today)
            endDate: DateTime.UtcNow.Date.AddYears(1) // Default end date (1 year from now)
        );

        if (!enabled)
            contract.Disable();

        return contract;
    }

    private record ContractDto(Guid Id, string Code, Guid PartnerId, DateTime StartDate, DateTime EndDate, bool IsEnabled);

    [Fact(DisplayName = "GET /api/contracts/{id} → 404 when Contract not found")]
    public async Task Get_ShouldReturn404_WhenContractNotFound()
    {
        var id = Guid.NewGuid();

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Contract?)null);

        var res = await _client.GetAsync($"/api/contracts/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _contractRepoMock.Verify(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/contracts/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/contracts/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _contractRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ContractId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/contracts/{id} → 200 for disabled Contract")]
    public async Task Get_ShouldReturn200_WhenContractDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "CONTRACT-DIS", enabled: false);

        _contractRepoMock.Setup(r => r.GetByIdAsync(ContractId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/contracts/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ContractDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}