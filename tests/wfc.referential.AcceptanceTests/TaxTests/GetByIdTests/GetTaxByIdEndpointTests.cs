using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.TaxAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.GetByIdTests;

public class GetTaxByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Tax Make(Guid id, string code = "TAX-001", string? name = null, bool enabled = true)
    {
        var tax = Tax.Create(
            id: TaxId.Of(id),
            code: code,
            codeEn: $"{code}-EN",
            codeAr: $"{code}-AR",
            description: name ?? $"Tax-{code}",
            fixedAmount: 5.0,
            rate: 0.10
        );

        if (!enabled)
            tax.SetInactive();

        return tax;
    }

    private record TaxDto(Guid Id, string Code, string Description, bool IsEnabled);

    [Fact(DisplayName = "GET /api/taxes/{id} → 200 when Tax exists")]
    public async Task Get_ShouldReturn200_WhenTaxExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "TAX-123", "VAT Tax");

        _taxRepoMock.Setup(r => r.GetByIdAsync(TaxId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/taxes/{id}");
        var body = await res.Content.ReadFromJsonAsync<TaxDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(id);
        body.Code.Should().Be("TAX-123");
        body.Description.Should().Be("VAT Tax");
        body.IsEnabled.Should().BeTrue();

        _taxRepoMock.Verify(r => r.GetByIdAsync(TaxId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/taxes/{id} → 404 when Tax not found")]
    public async Task Get_ShouldReturn404_WhenTaxNotFound()
    {
        var id = Guid.NewGuid();

        _taxRepoMock.Setup(r => r.GetByIdAsync(TaxId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Tax?)null);

        var res = await _client.GetAsync($"/api/taxes/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _taxRepoMock.Verify(r => r.GetByIdAsync(TaxId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/taxes/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/taxes/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _taxRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TaxId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/taxes/{id} → 200 for disabled Tax")]
    public async Task Get_ShouldReturn200_WhenTaxDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "TAX-DIS", enabled: false);

        _taxRepoMock.Setup(r => r.GetByIdAsync(TaxId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/taxes/{id}");
        var dto = await res.Content.ReadFromJsonAsync<TaxDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}