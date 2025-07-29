using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyDenominationTests.GetByIdTests;

public class GetCurrencyDenominationByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static CurrencyDenomination Make(Guid id,Guid currencyid, CurrencyDenominationType type , decimal value = 0, bool enabled = true)
    {
        var currencyDenomination = CurrencyDenomination.Create(
            id: CurrencyDenominationId.Of(id),
            currencyId: CurrencyId.Of(currencyid),
            typeCurrency: CurrencyDenominationType.Banknote,
            value: value
            );

        if (!enabled)
            currencyDenomination.Disable();

        return currencyDenomination;
    }

    private record CurrencyDenominationDto(Guid Id,Guid currencyId, CurrencyDenominationType Type, decimal Value, bool IsEnabled);

    [Fact(DisplayName = "GET /api/currencyDenominations/{id} → 404 when Currency not found")]
    public async Task Get_ShouldReturn404_WhenCurrencyDenominationNotFound()
    {
        var id = Guid.NewGuid();

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(CurrencyDenominationId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((CurrencyDenomination?)null);

        var res = await _client.GetAsync($"/api/currencyDenominations/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _currencyDenominationRepoMock.Verify(r => r.GetByIdAsync(CurrencyDenominationId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencyDenominations/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/currencyDenominations/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _currencyDenominationRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyDenominationId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/currencyDenominations/{id} → 200 for disabled Currency")]
    public async Task Get_ShouldReturn200_WhenCurrencyDenominationDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, Guid.NewGuid() ,CurrencyDenominationType.Banknote, enabled: false);

        _currencyDenominationRepoMock.Setup(r => r.GetByIdAsync(CurrencyDenominationId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/currencyDenominations/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CurrencyDenominationDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}