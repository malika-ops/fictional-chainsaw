using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CurrencyAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CurrencyTests.GetByIdTests;

public class GetCurrencyByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Currency Make(Guid id, string code = "USD", string? name = null, bool enabled = true)
    {
        var currency = Currency.Create(
            id: CurrencyId.Of(id),
            code: code,
            codeAR: $"{code}-AR",
            codeEN: $"{code}-EN",
            name: name ?? $"Currency-{code}",
            codeIso: 840
        );

        if (!enabled)
            currency.Disable();

        return currency;
    }

    private record CurrencyDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/currencies/{id} → 404 when Currency not found")]
    public async Task Get_ShouldReturn404_WhenCurrencyNotFound()
    {
        var id = Guid.NewGuid();

        _currencyRepoMock.Setup(r => r.GetByIdAsync(CurrencyId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Currency?)null);

        var res = await _client.GetAsync($"/api/currencies/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _currencyRepoMock.Verify(r => r.GetByIdAsync(CurrencyId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/currencies/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/currencies/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _currencyRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/currencies/{id} → 200 for disabled Currency")]
    public async Task Get_ShouldReturn200_WhenCurrencyDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "XXX", enabled: false);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(CurrencyId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/currencies/{id}");
        var dto = await res.Content.ReadFromJsonAsync<CurrencyDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}