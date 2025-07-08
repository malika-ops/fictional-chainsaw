using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.PricingAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractDetailsTests.GetByIdTests;

public class GetContractDetailByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static ContractDetails Make(Guid id, string code = "CONTRACT-DETAIL-001", string? name = null, bool enabled = true)
    {
        var contractDetail = ContractDetails.Create(
            id: ContractDetailsId.Of(id),
            contractId: ContractId.Of(Guid.NewGuid()), // Default contract ID
            pricingId: PricingId.Of(Guid.NewGuid()) // Default pricing ID
        );

        if (!enabled)
            contractDetail.Disable();

        return contractDetail;
    }

    private record ContractDetailDto(Guid Id, Guid ContractId, Guid PricingId, bool IsEnabled);

    [Fact(DisplayName = "GET /api/contractdetails/{id} → 404 when ContractDetail not found")]
    public async Task Get_ShouldReturn404_WhenContractDetailNotFound()
    {
        var id = Guid.NewGuid();

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ContractDetails?)null);

        var res = await _client.GetAsync($"/api/contractdetails/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _contractDetailsRepoMock.Verify(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/contractdetails/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/contractdetails/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _contractDetailsRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ContractDetailsId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/contractdetails/{id} → 200 for disabled ContractDetail")]
    public async Task Get_ShouldReturn200_WhenContractDetailDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "CONTRACT-DETAIL-DIS", enabled: false);

        _contractDetailsRepoMock.Setup(r => r.GetByIdAsync(ContractDetailsId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/contractdetails/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ContractDetailDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}