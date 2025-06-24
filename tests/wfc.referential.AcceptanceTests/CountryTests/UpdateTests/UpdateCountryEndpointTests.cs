using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

using System.Runtime.Serialization;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.AcceptanceTests.CountryTests.UpdateTests;

public class UpdateCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private readonly Mock<ICountryRepository> _countryRepo = new();
    private readonly Mock<ICurrencyRepository> _currencyRepo = new();
    private readonly Mock<IMonetaryZoneRepository> _zoneRepo = new();

    public UpdateCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<ICountryRepository>();
                s.RemoveAll<ICurrencyRepository>();
                s.RemoveAll<IMonetaryZoneRepository>();
                s.RemoveAll<ICacheService>();

                _countryRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

                s.AddSingleton(_countryRepo.Object);
                s.AddSingleton(_currencyRepo.Object);
                s.AddSingleton(_zoneRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

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

        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 409 when Code duplicate")]
    public async Task Put_ShouldReturn409_WhenCodeDuplicate()
    {
        var idTarget = Guid.NewGuid();
        var idExisting = Guid.NewGuid();

        var target = MakeCountry(idTarget, code: "OLD");
        var existing = MakeCountry(idExisting, code: "DUPL");

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(idTarget), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(target);
        _countryRepo.Setup(r => r.GetOneByConditionAsync(
                               It.IsAny<System.Linq.Expressions.Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existing);

        _currencyRepo.Setup(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Currency)) as Currency);
        _zoneRepo.Setup(r => r.GetByIdAsync(It.IsAny<MonetaryZoneId>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(MonetaryZone)) as MonetaryZone);

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
        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 404 when Currency missing")]
    public async Task Put_ShouldReturn404_WhenCurrencyMissing()
    {
        var id = Guid.NewGuid();
        var cur = Guid.NewGuid();
        var mz = Guid.NewGuid();
        var cty = MakeCountry(id);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(cty);
        _countryRepo.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        _currencyRepo.Setup(r => r.GetByIdAsync(CurrencyId.Of(cur), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Currency?)null);          
        _zoneRepo.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(mz), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(MonetaryZone)) as MonetaryZone);

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

        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/countries/{id} → 404 when MonetaryZone missing")]
    public async Task Put_ShouldReturn404_WhenZoneMissing()
    {
        var id = Guid.NewGuid();
        var cur = Guid.NewGuid();
        var mz = Guid.NewGuid();
        var cty = MakeCountry(id);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(cty);
        _countryRepo.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                               System.Linq.Expressions.Expression<Func<Country, bool>>>(),
                               It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        _currencyRepo.Setup(r => r.GetByIdAsync(CurrencyId.Of(cur), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Currency)) as Currency);
        _zoneRepo.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(mz), It.IsAny<CancellationToken>()))
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
        _countryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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