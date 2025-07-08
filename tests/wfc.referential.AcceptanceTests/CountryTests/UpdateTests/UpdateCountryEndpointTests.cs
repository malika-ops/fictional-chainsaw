using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.UpdateTests;

public class UpdateCountryEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Country MakeCountry(Guid id, string code = "AAA", bool enabled = true)
    {
        var mzId = MonetaryZoneId.Of(Guid.NewGuid());
        var cur = CurrencyId.Of(Guid.NewGuid());

        return Country.Create(
            CountryId.Of(id),
            abbreviation: "ABR",
            name: "Country-Old",
            code: code,
            ISO2: "XY",
            ISO3: "XYZ",
            dialingCode: "+111",
            timeZone: "UTC",
            hasSector: false,
            isSmsEnabled: false,
            numberDecimalDigits: 2,
            monetaryZoneId: mzId,
            currencyId: cur);
    }


    [Fact(DisplayName = "PUT /api/countries/{id} → 400 when Code empty")]
    public async Task Put_ShouldReturn400_WhenCodeEmpty()
    {
        var id = Guid.NewGuid();

        var payload = new
        {
            CountryId = id,
            Name = "X",
            Code = "",               
            ISO2 = "AA",
            ISO3 = "AAA",
            DialingCode = "+1",
            TimeZone = "UTC",
            NumberDecimalDigits = 2,
            MonetaryZoneId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/countries/{id}", payload);
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 409 when Code duplicate")]
    public async Task Put_ShouldReturn409_WhenCodeDuplicate()
    {
        var idTarget = Guid.NewGuid();
        var idExisting = Guid.NewGuid();

        var target = MakeCountry(idTarget, code: "OLD");
        var existing = MakeCountry(idExisting, code: "DUPL");

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(idTarget), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(target);
        _countryRepoMock.Setup(r => r.GetOneByConditionAsync(
                               It.IsAny<System.Linq.Expressions.Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existing);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(_fixture.Create<Currency>());
        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<MonetaryZoneId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_fixture.Create<MonetaryZone>());

        var payload = new
        {
            CountryId = idTarget,
            Name = "X",
            Code = "DUPL",
            ISO2 = "DU",
            ISO3 = "DUP",
            DialingCode = "+1",
            TimeZone = "UTC",
            NumberDecimalDigits = 2,
            MonetaryZoneId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/countries/{idTarget}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 404 when Currency missing")]
    public async Task Put_ShouldReturn404_WhenCurrencyMissing()
    {
        var id = Guid.NewGuid();
        var cur = Guid.NewGuid();
        var mz = Guid.NewGuid();
        var cty = MakeCountry(id);

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(cty);
        _countryRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(CurrencyId.Of(cur), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Currency?)null);          
        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(mz), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(_fixture.Create<MonetaryZone>());

        var payload = new
        {
            CountryId = id,
            Name = "X",
            Code = "NEW",
            ISO2 = "NW",
            ISO3 = "NEW",
            DialingCode = "+1",
            TimeZone = "UTC",
            NumberDecimalDigits = 2,
            MonetaryZoneId = mz,
            CurrencyId = cur
        };

        var res = await _client.PutAsJsonAsync($"/api/countries/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 404 when MonetaryZone missing")]
    public async Task Put_ShouldReturn404_WhenZoneMissing()
    {
        var id = Guid.NewGuid();
        var cur = Guid.NewGuid();
        var mz = Guid.NewGuid();
        var cty = MakeCountry(id);

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(cty);
        _countryRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        _currencyRepoMock.Setup(r => r.GetByIdAsync(CurrencyId.Of(cur), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(_fixture.Create<Currency>());
        _monetaryZoneRepoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(mz), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((MonetaryZone?)null);

        var payload = new
        {
            CountryId = id,
            Name = "X",
            Code = "NEW",
            ISO2 = "NW",
            ISO3 = "NEW",
            DialingCode = "+1",
            TimeZone = "UTC",
            NumberDecimalDigits = 2,
            MonetaryZoneId = mz,
            CurrencyId = cur
        };

        var res = await _client.PutAsJsonAsync($"/api/countries/{id}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 400 when CountryId empty")]
    public async Task Put_ShouldReturn400_WhenCountryIdEmpty()
    {
        var payload = new
        {
            CountryId = Guid.Empty,
            Name = "X",
            Code = "X",
            ISO2 = "AA",
            ISO3 = "AAA",
            DialingCode = "+1",
            TimeZone = "UTC",
            NumberDecimalDigits = 2,
            MonetaryZoneId = Guid.NewGuid(),
            CurrencyId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync(
            "/api/countries/00000000-0000-0000-0000-000000000000",
            payload);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}