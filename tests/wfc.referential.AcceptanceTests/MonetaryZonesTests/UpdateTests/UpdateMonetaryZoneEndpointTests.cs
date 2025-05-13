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

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.UpdateTests;

public class UpdateMonetaryZoneEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repoMock = new();


    public UpdateMonetaryZoneEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMonetaryZoneRepository>();
                services.RemoveAll<ICacheService>();

                // default noop for Update
                _repoMock
                    .Setup(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                          It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // helper to create a MonetaryZone quickly
    private static MonetaryZone Zone(Guid id, string code, string name) =>
        MonetaryZone.Create(MonetaryZoneId.Of(id), code, name, "desc",
                             new List<Country>());

    // 1) Happy‑path update
    [Fact(DisplayName = "PUT /api/monetaryZones/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldZone = Zone(id, "SEK", "Swedish Krona");

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldZone);

        _repoMock.Setup(r => r.GetByCodeAsync("NOK", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((MonetaryZone?)null);   // code is unique

        MonetaryZone? updated = null;
        _repoMock.Setup(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                       It.IsAny<CancellationToken>()))
                 .Callback<MonetaryZone, CancellationToken>((mz, _) => updated = mz)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            MonetaryZoneId = id,
            Code = "NOK",
            Name = "Norwegian Krone",
            Description = "Norway currency"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("NOK");
        updated.Name.Should().Be("Norwegian Krone");
        updated.Description.Should().Be("Norway currency");

        _repoMock.Verify(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Validation error – Name missing
    [Fact(DisplayName = "PUT /api/monetaryZones/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            MonetaryZoneId = id,
            Code = "USD",
            // Name omitted
            Description = "desc"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoMock.Verify(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 3) Duplicate code
    [Fact(DisplayName = "PUT /api/monetaryZones/{id} returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = Zone(Guid.NewGuid(), "EUR", "Euro");
        var target = Zone(id, "USD", "US Dollar");

        _repoMock.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("EUR", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // duplicate code

        var payload = new
        {
            MonetaryZoneId = id,
            Code = "EUR",          // duplicate
            Name = "Euro",
            Description = "desc"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/monetaryZones/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("MonetaryZone with code EUR already exists.");

        _repoMock.Verify(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                        It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
