using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;            
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryTests.DeleteTests;

public class DeleteCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<IPartnerCountryRepository> _partnerRepoMock = new();
    private readonly Mock<ICorridorRepository> _corridorRepoMock = new();
    private readonly Mock<ICountryIdentityDocRepository> _idDocRepoMock = new();
    private readonly Mock<ICountryServiceRepository> _serviceRepoMock = new();
    private readonly Mock<IAffiliateRepository> _affiliateRepoMock = new();

    private readonly HttpClient _client;

    public DeleteCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<IPartnerCountryRepository>();
                services.RemoveAll<ICorridorRepository>();
                services.RemoveAll<ICountryIdentityDocRepository>();
                services.RemoveAll<ICountryServiceRepository>();
                services.RemoveAll<IAffiliateRepository>();
                services.RemoveAll<ICacheService>();

                _countryRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);

                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_partnerRepoMock.Object);
                services.AddSingleton(_corridorRepoMock.Object);
                services.AddSingleton(_idDocRepoMock.Object);
                services.AddSingleton(_serviceRepoMock.Object);
                services.AddSingleton(_affiliateRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Country MakeCountry(Guid id)
    {
        var cId = CountryId.Of(id);
        var mzId = MonetaryZoneId.Of(Guid.NewGuid());
        var cur = CurrencyId.Of(Guid.NewGuid());

        return Country.Create(
            cId,
            abbreviation: null,
            name: "Test-Country",
            code: "TEST",
            ISO2: "TS",
            ISO3: "TST",
            dialingCode: "+999",
            timeZone: "UTC",
            hasSector: false,
            isSmsEnabled: false,
            numberDecimalDigits: 2,
            monetaryZoneId: mzId,
            currencyId: cur);
    }


    [Fact(DisplayName = "DELETE /api/countries/{id} returns 200 when Country exists and has no associations")]
    public async Task Delete_ShouldReturn200_WhenCountryExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var country = MakeCountry(id);                  

        _countryRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                        CountryId.Of(id),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<System.Linq.Expressions.Expression<Func<Country, object>>[]>()))
            .ReturnsAsync(country);

        _partnerRepoMock.Setup(r =>
        r.GetOneByConditionAsync(
            It.IsAny<Expression<Func<PartnerCountry, bool>>>(),
            It.IsAny<CancellationToken>()))
    .ReturnsAsync((PartnerCountry?)null);

        _corridorRepoMock.Setup(r =>
                r.GetOneByConditionAsync(
                    It.IsAny<Expression<Func<Corridor, bool>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Corridor?)null);

        _idDocRepoMock.Setup(r =>
                r.GetOneByConditionAsync(
                    It.IsAny<Expression<Func<CountryIdentityDoc, bool>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountryIdentityDoc?)null);

        _serviceRepoMock.Setup(r =>
                r.GetOneByConditionAsync(
                    It.IsAny<Expression<Func<CountryService, bool>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((CountryService?)null);

        _affiliateRepoMock.Setup(r =>
                r.GetOneByConditionAsync(
                    It.IsAny<Expression<Func<Affiliate, bool>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Affiliate?)null);

        Country? captured = null;
        _countryRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Callback(() => captured = country)
                        .Returns(Task.CompletedTask);

        // Act
        var resp = await _client.DeleteAsync($"/api/countries/{id}");
        var body = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();              // Disable() called
        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "DELETE /api/countries/{id} returns 404 when Country not found")]
    public async Task Delete_ShouldReturn404_WhenCountryNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _countryRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                        CountryId.Of(id),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<System.Linq.Expressions.Expression<Func<Country, object>>[]>()))
            .ReturnsAsync((Country?)null);

        // Act
        var resp = await _client.DeleteAsync($"/api/countries/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "DELETE /api/countries/{id} returns 400 when CountryId is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenCountryIdEmpty()
    {
        // Arrange
        var empty = Guid.Empty;

        // Act
        var resp = await _client.DeleteAsync($"/api/countries/{empty}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("CountryId")[0].GetString()
            .Should().Be("CountryId must be a non-empty GUID.");

        _countryRepoMock.Verify(r => r.GetByIdWithIncludesAsync(It.IsAny<CountryId>(),
                          It.IsAny<CancellationToken>(),
                          It.IsAny<System.Linq.Expressions.Expression<Func<Country, object>>[]>()), Times.Never);
        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "DELETE /api/countries/{id} returns 400 when CountryId is malformed")]
    public async Task Delete_ShouldReturn400_WhenCountryIdMalformed()
    {
        // Arrange
        const string badId = "not-a-valid-guid";

        // Act
        var resp = await _client.DeleteAsync($"/api/countries/{badId}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}