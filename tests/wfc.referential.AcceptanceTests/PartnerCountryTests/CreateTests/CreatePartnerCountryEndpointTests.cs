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
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using Xunit;


using System.Runtime.Serialization;

namespace wfc.referential.AcceptanceTests.PartnerCountryTests.CreateTests;

public class CreatePartnerCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();
    private readonly Mock<IPartnerRepository> _partnerMock = new();
    private readonly Mock<ICountryRepository> _countryMock = new();

    public CreatePartnerCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<IPartnerRepository>();
                s.RemoveAll<ICountryRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.AddAsync(It.IsAny<PartnerCountry>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((PartnerCountry pc, CancellationToken _) => pc);

                _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                var dummyPartner = FormatterServices.GetUninitializedObject(typeof(Partner)) as Partner;
                var dummyCountry = FormatterServices.GetUninitializedObject(typeof(Country)) as Country;

                _partnerMock.Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(dummyPartner);

                _countryMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(dummyCountry);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(_partnerMock.Object);
                s.AddSingleton(_countryMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    private static object Payload(Guid partnerId, Guid countryId) => new
    {
        PartnerId = partnerId,
        CountryId = countryId
    };

    private static PartnerCountry CreateLink(Guid partnerId, Guid countryId) =>
        PartnerCountry.Create(
            PartnerCountryId.Of(Guid.NewGuid()),
            PartnerId.Of(partnerId),
            CountryId.Of(countryId));


    [Fact(DisplayName = "POST /api/partner-countries → 200 + Guid on valid request")]
    public async Task Post_ShouldReturn200_AndGuid_WhenValid()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var payload = Payload(partnerId, countryId);

        // Act
        var resp = await _client.PostAsJsonAsync("/api/partner-countries", payload);
        var id = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        id.Should().NotBeEmpty();

        _repoMock.Verify(r =>
            r.AddAsync(It.Is<PartnerCountry>(pc =>
                    pc.PartnerId.Value == partnerId &&
                    pc.CountryId.Value == countryId),
                It.IsAny<CancellationToken>()), Times.Once);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "POST /api/partner-countries → 400 when PartnerId is empty")]
    public async Task Post_ShouldReturn400_WhenPartnerIdEmpty()
    {
        var payload = Payload(Guid.Empty, Guid.NewGuid());

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("PartnerId")[0].GetString()
            .Should().Be("PartnerId is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partner-countries → 400 when CountryId is empty")]
    public async Task Post_ShouldReturn400_WhenCountryIdEmpty()
    {
        var payload = Payload(Guid.NewGuid(), Guid.Empty);

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors")
            .GetProperty("CountryId")[0].GetString()
            .Should().Be("CountryId is required.");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partner-countries → 409 when link already exists")]
    public async Task Post_ShouldReturn409_WhenDuplicate()
    {
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var existing = CreateLink(partnerId, countryId);

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<
                         System.Linq.Expressions.Expression<Func<PartnerCountry, bool>>>(),
                         It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);      

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", Payload(partnerId, countryId));

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partner-countries → 404 when Partner not found")]
    public async Task Post_ShouldReturn404_WhenPartnerMissing()
    {
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        _partnerMock.Setup(r => r.GetByIdAsync(PartnerId.Of(partnerId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Partner?)null);

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", Payload(partnerId, countryId));

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "POST /api/partner-countries → 404 when Country not found")]
    public async Task Post_ShouldReturn404_WhenCountryMissing()
    {
        var partnerId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        var dummyPartner = FormatterServices.GetUninitializedObject(typeof(Partner)) as Partner;
        _partnerMock.Setup(r => r.GetByIdAsync(PartnerId.Of(partnerId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dummyPartner);

        _countryMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Country?)null);

        var resp = await _client.PostAsJsonAsync("/api/partner-countries", Payload(partnerId, countryId));

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<PartnerCountry>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}