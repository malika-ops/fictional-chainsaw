using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Regions.Dtos;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.UpdateTests;

public class UpdateRegionEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Region DummyRegion(Guid id, string code, string name) =>
        Region.Create(RegionId.Of(id), code, name, CountryId.Of(Guid.NewGuid()));

    [Fact(DisplayName = "PUT /api/regions/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var oldRegion = DummyRegion(id, "NA", "North America");

        _regionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Region, bool>> predicate, CancellationToken _) =>
            {
                var func = predicate.Compile();

                if (func(oldRegion))
                    return oldRegion;

                return null;
            });
        _countryRepoMock.Setup(
            r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Country.Create(
                CountryId.Of(Guid.NewGuid()),
                "Morocco",
                "MA",
                "Moroccan Dirham", "iso2", "iso3", "dialingCode", "GMT", true, true, 2,
                MonetaryZoneId.Of(Guid.NewGuid()), CurrencyId.Of(Guid.NewGuid())));

        Region? updated = null;
        _regionRepoMock.Setup(r => r.Update(oldRegion))
                 .Callback<Region>((rg) => updated = rg);

        var payload = new UpdateRegionRequest
        {
            Code = "SA",
            Name = "South America",
            IsEnabled= true,
            CountryId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"api/regions/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(true);

        updated!.Code.Should().Be("SA");
        updated.Name.Should().Be("South America");

        _regionRepoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/regions/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        var id = Guid.NewGuid();
        var payload = new UpdateRegionRequest
        {
            Code = "AF",
            CountryId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"/api/regions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("Name")[0].GetString()
            .Should().Be("Name is required");

        _regionRepoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/regions/{id} returns 409 Conflict when new code already exists")]
    public async Task Put_ShouldReturn409Conflict_WhenCodeAlreadyExists()
    {
        var id = Guid.NewGuid();
        var existing = DummyRegion(Guid.NewGuid(), "EU", "Europe");
        var target = DummyRegion(id, "AS", "Asia");


        _regionRepoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Region, bool>> predicate, CancellationToken _) =>
            {
                var func = predicate.Compile();

                if (func(target))
                    return target;

                return existing;
            });

        var payload = new UpdateRegionRequest
        {
            Code = "EU", // duplicate code
            Name = "Europe",
            IsEnabled = true,
            CountryId = Guid.NewGuid()
        };

        var response = await _client.PutAsJsonAsync($"/api/regions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        doc!.RootElement.GetProperty("errors").GetProperty("message").GetString()
           .Should().Be($"{nameof(Region)} with code : {payload.Code} already exist");

        _regionRepoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Never);
    }
}
