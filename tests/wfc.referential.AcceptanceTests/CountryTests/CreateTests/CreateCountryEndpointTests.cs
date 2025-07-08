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

namespace wfc.referential.AcceptanceTests.CountryTests.CreateTests;

public class CreateCountryEndpointTests : BaseAcceptanceTests
{

    public CreateCountryEndpointTests(TestWebApplicationFactory factory) : base(factory)
    {
        var dummyCurrency = _fixture.Create<Currency>();
        var dummyMonetaryZone = _fixture.Create<MonetaryZone>();

        _currencyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<CurrencyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyCurrency);

        _monetaryZoneRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<MonetaryZoneId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyMonetaryZone);
    }
   

    private static object ValidPayload(Guid? currencyId = null, Guid? zoneId = null, string? code = null)
    {
        return new
        {
            Abbreviation = "USA",
            Name = "United States",
            Code = code ?? "US",
            ISO2 = "US",
            ISO3 = "USA",
            DialingCode = "+1",
            TimeZone = "+5",
            HasSector = true,
            IsSmsEnabled = false,
            NumberDecimalDigits = 2,
            MonetaryZoneId = zoneId ?? Guid.NewGuid(),
            CurrencyId = currencyId ?? Guid.NewGuid()
        };
    }


    [Fact(DisplayName = "POST /api/countries → 200 & Guid on valid request")]
    public async Task Post_ShouldReturn200_AndId_WhenValid()
    {
        // Arrange
        var payload = ValidPayload();

        // Act
        var resp = await _client.PostAsJsonAsync("/api/countries", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _countryRepoMock.Verify(r =>
            r.AddAsync(It.Is<Country>(c =>
                    c.Name == payload.GetType().GetProperty("Name")!.GetValue(payload)!.ToString() &&
                    c.Code == "US" &&
                    c.ISO2 == "US" &&
                    c.ISO3 == "USA"),
                It.IsAny<CancellationToken>()), Times.Once);

        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/countries → 400 when ISO2 length not 2")]
    public async Task Post_ShouldReturn400_WhenISO2WrongLength()
    {
        var bad = ValidPayload(code: "USBad");
        var payload = new
        {
            ((dynamic)bad).Abbreviation,
            ((dynamic)bad).Name,
            ((dynamic)bad).Code,
            ISO2 = "USA",       // 3 chars instead of 2
            ((dynamic)bad).ISO3,
            ((dynamic)bad).DialingCode,
            ((dynamic)bad).TimeZone,
            ((dynamic)bad).HasSector,
            ((dynamic)bad).NumberDecimalDigits,
            ((dynamic)bad).MonetaryZoneId,
            ((dynamic)bad).CurrencyId
        };

        var resp = await _client.PostAsJsonAsync("/api/countries", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _countryRepoMock.Verify(r => r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/countries → 409 when Code already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicateCode()
    {
        const string dupCode = "US";

        // set repo to return an existing country with the same Code
        var existing = Country.Create(
            CountryId.Of(Guid.NewGuid()),
            "USA",
            "United States",
            dupCode,
            "US",
            "USA",
            "+1",
            "+5",
            false,
            false,
            2,
            MonetaryZoneId.Of(Guid.NewGuid()),
            CurrencyId.Of(Guid.NewGuid()));

        _countryRepoMock.Setup(r => r.GetOneByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Country, bool>>>(),
            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(existing);

        var resp = await _client.PostAsJsonAsync("/api/countries", ValidPayload(code: dupCode));

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _countryRepoMock.Verify(r => r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/countries → 404 when MonetaryZone missing")]
    public async Task Post_ShouldReturn404_WhenMonetaryZoneNotFound()
    {
        var zoneId = Guid.NewGuid();

        _monetaryZoneRepoMock
            .Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(zoneId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonetaryZone?)null);   // simulate missing zone

        var resp = await _client.PostAsJsonAsync("/api/countries", ValidPayload(zoneId: zoneId));

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryRepoMock.Verify(r => r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/countries → 404 when Currency missing")]
    public async Task Post_ShouldReturn404_WhenCurrencyNotFound()
    {
        var curId = Guid.NewGuid();

        _currencyRepoMock
            .Setup(r => r.GetByIdAsync(CurrencyId.Of(curId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);   // simulate missing currency

        var resp = await _client.PostAsJsonAsync("/api/countries", ValidPayload(currencyId: curId));

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryRepoMock.Verify(r => r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/countries → 400 when NumberDecimalDigits out of range")]
    public async Task Post_ShouldReturn400_WhenDecimalsOutOfRange()
    {
        var payload = ValidPayload();
        var bad = new
        {
            ((dynamic)payload).Abbreviation,
            ((dynamic)payload).Name,
            ((dynamic)payload).Code,
            ((dynamic)payload).ISO2,
            ((dynamic)payload).ISO3,
            ((dynamic)payload).DialingCode,
            ((dynamic)payload).TimeZone,
            ((dynamic)payload).HasSector,
            NumberDecimalDigits = 0,  // invalid (<1)
            ((dynamic)payload).MonetaryZoneId,
            ((dynamic)payload).CurrencyId
        };

        var resp = await _client.PostAsJsonAsync("/api/countries", bad);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _countryRepoMock.Verify(r => r.AddAsync(It.IsAny<Country>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}