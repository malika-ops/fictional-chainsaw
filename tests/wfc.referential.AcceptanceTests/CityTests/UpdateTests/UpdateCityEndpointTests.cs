using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CityTests.UpdateTests;

public class UpdateCityEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICityRepository> _repoCityMock = new();
    private readonly Mock<IRegionRepository> _repoRegionMock = new();


    public UpdateCityEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRegionRepository>();
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // default noop for Update
                _repoCityMock
                    .Setup(r => r.UpdateCityAsync(It.IsAny<City>(),
                                                          It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoCityMock.Object);
                services.AddSingleton(_repoRegionMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to create a City quickly
    private static City DummyCity(Guid id, string code, string name) =>
        City.Create(CityId.Of(id), code, name, "timezone", RegionId.Of(Guid.NewGuid()), "abbrev");

    // 1) Happy‑path update
    [Fact(DisplayName = "PUT /api/cities/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldCity = DummyCity(id, "SEK", "Swedish Krona");

        //_repoCityMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(oldCity);
        City? updated = null;
        _repoCityMock.Setup(r => r.UpdateCityAsync(oldCity,
                                                       It.IsAny<CancellationToken>()))
                 .Callback<City, CancellationToken>((city, _) => updated = city)
                 .Returns(Task.CompletedTask);

        var payload = new UpdateCityRequest
        {
            Code= "codeAAB",
            Name= "nameAAB",
            Abbreviation= "NYC",
            RegionId= Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone= "America/New_York",
            IsEnabled = true
        };
        _repoRegionMock
            .Setup(r => r.GetByIdAsync(payload.RegionId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Region.Create(RegionId.Of(oldCity.RegionId.Value), "code", "name", CountryId.Of(Guid.NewGuid())));

        _repoCityMock.Setup(r => r.GetByCodeAsync(payload.Code, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((City)null);


        // Act
        var response = await _client.PutAsJsonAsync($"/api/cities/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("codeAAB");
        updated.Name.Should().Be("nameAAB");

        _repoCityMock.Verify(r => r.UpdateCityAsync(It.IsAny<City>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Validation error – Name missing
    [Fact(DisplayName = "PUT /api/cities/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new UpdateCityRequest
        {
            Code = "codeAAB",
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/cities/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoCityMock.Verify(r => r.UpdateCityAsync(It.IsAny<City>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    ////// 3) Duplicate code
    [Fact(DisplayName = "PUT /api/cities/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        // Arrange
        var id = Guid.NewGuid();
        var existing = City.Create
        (
            CityId.Of(Guid.NewGuid()),
            "codeAAB",
            "Name",
            "NYC",
            RegionId.Of(Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060")),
            "America/New_York"
        );
        var target = City.Create
        (
            CityId.Of(id),
            "target",
            "targetName",
            "targetNYC",
            RegionId.Of(Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060")),
            "America/New_York"
        );

        //_repoCityMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(target);

        _repoCityMock.Setup(r => r.GetByCodeAsync("target", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // duplicate code

        var payload = new UpdateCityRequest
        {
            CityId = id,
            Code = "target",
            Name = "Name",
            Abbreviation = "NYC",
            RegionId = Guid.Parse("a3b8b953-2489-49c5-aac4-c97df06d5060"),
            TimeZone = "America/New_York",
            IsEnabled = true
        };

        _repoRegionMock
            .Setup(r => r.GetByIdAsync(payload.RegionId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Region.Create(RegionId.Of(payload.RegionId.Value), "code", "name", CountryId.Of(Guid.NewGuid())));


        // Act
        var response = await _client.PutAsJsonAsync($"/api/cities/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"{nameof(Region)} with code : {payload.Code} already exist");

        _repoCityMock.Verify(r => r.UpdateCityAsync(It.IsAny<City>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
