using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CorridorTests;

public class CreateCorridorEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl = "api/corridors";

    [Fact(DisplayName = $"POST {BaseUrl} returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndGuid_WhenRequestIsValid()
    {
        var payload = new
        {
            SourceCountryId = Guid.NewGuid()
        };

        _countryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Country.Create(
            new CountryId(Guid.NewGuid()), "FR","France","FRA","FR","FRA","+33","GMT+1",false,false,2,new MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
        ));
        _cityRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<City, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(City.Create(CityId.Of(Guid.NewGuid()),"CA", "Casa", "t1", RegionId.Of(Guid.NewGuid()),"abbr"));
        _agencyRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Agency.Create(AgencyId.Of(Guid.NewGuid()),"code","name","-abbr","42, Main St.",null,
                "+212600000000","0522334455","Sheet-A","Acc-001","90001",null,null,null,null,null,null,null,
                CityId.Of(Guid.NewGuid()),null,null,null,null,null,null));

        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        //_repoMock.Verify(r => r.AddAsync(It.Is<Corridor>(c =>
        //    c.SourceCountryId.Value == payload.SourceCountryId &&
        //    c.DestinationCountryId.Value == payload.DestinationCountryId
        //), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = $"POST {BaseUrl} returns 400 when No value was provided ")]
    public async Task Post_ShouldReturn400_WhenNoValueWasProvided()
    {
        var payload = new CreateCorridorRequest
        {

        };

        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = doc!.RootElement.GetProperty("errors");
        errors.GetProperty("")[0].GetString().Should().Be("At least one of the source or destination fields must be provided.");

        _corridorRepoMock.Verify(r => r.AddAsync(It.IsAny<Corridor>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
