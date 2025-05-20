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
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.DeleteTests;

public class DeleteSectorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _repoMock = new();

    public DeleteSectorEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy sectors quickly
    private static Sector CreateTestSector(Guid id, string code, string name)
    {
        var country = Country.Create(
            new CountryId(Guid.NewGuid()),
            "TC", "Test Country", "TC", "TC", "TCO", "+0", "0", false, false, 2,
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
        );

        var city = City.Create(
            new CityId(Guid.NewGuid()),
            "CITY1",
            "Test City",
            "GMT",
            new Domain.RegionAggregate.RegionId(Guid.NewGuid()),
            "TC"
        );

        return Sector.Create(new SectorId(id), code, name, city);
    }

    [Fact(DisplayName = "DELETE /api/sectors/{id} returns 200 when sector exists and has no agencies")]
    public async Task Delete_ShouldReturn200_WhenSectorExistsAndHasNoAgencies()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sector = CreateTestSector(id, "TEST-001", "Test Sector");

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        //_repoMock
        //    .Setup(r => r.HasLinkedAgenciesAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(false);

        // Capture the entity passed to Update
        Sector? updatedSector = null;
        _repoMock
            .Setup(r => r.UpdateSectorAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()))
            .Callback<Sector, CancellationToken>((s, _) => updatedSector = s)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedSector!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/sectors/{id} returns 400 when sector is not found")]
    public async Task Delete_ShouldReturn400_WhenSectorNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Sector not found");

        _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Never);
    }

    //[Fact(DisplayName = "DELETE /api/sectors/{id} returns 400 when sector has linked agencies")]
    //public async Task Delete_ShouldReturn400_WhenSectorHasLinkedAgencies()
    //{
    //    // Arrange
    //    var id = Guid.NewGuid();
    //    var sector = CreateTestSector(id, "TEST-002", "Test Sector");

    //    _repoMock
    //        .Setup(r => r.GetByIdAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(sector);

    //    //_repoMock
    //    //    .Setup(r => r.HasLinkedAgenciesAsync(It.Is<SectorId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
    //    //    .ReturnsAsync(true);  // Has linked agencies

    //    // Act
    //    var response = await _client.DeleteAsync($"/api/sectors/{id}");
    //    var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    //    doc!.RootElement.GetProperty("errors").GetString()
    //       .Should().Be($"Cannot delete sector with ID {id} because it is linked to one or more agencies.");

    //    _repoMock.Verify(r => r.UpdateSectorAsync(It.IsAny<Sector>(),
    //                                             It.IsAny<CancellationToken>()),
    //                                             Times.Never);
    //}
}