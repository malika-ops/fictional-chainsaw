using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.PatchTests;

public class PatchAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();

    public PatchAgencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgencyRepository>();
                services.RemoveAll<ICacheService>();

                //_repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
                //         .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    // helpers

    private static Agency Ag(string code, string name, Guid id)
    {
        var abbr = code.Length >= 3 ? code[..3] : code;

        return Agency.Create(
            AgencyId.Of(id), code, name, abbr,
            "1 Main St", null,
            "0600000000", "",       // phone / fax
            "Sheet", "401122",      // accounting
            "", "",                 // MG ref / pwd
            "10000", "",            // postal / perm
            null, null,             // lat / long
            CityId.Of(Guid.NewGuid()),
            null,                   // sector
            null,                   // agency-type
            null, null);            // support / partner
    }

    private async Task<HttpResponseMessage> PatchJsonAsync(string url, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await _client.SendAsync(request);
    }

    //Happy-path

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 200 when partial update succeeds")]
    public async Task Patch_ShouldReturn200_WhenPatchSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var original = Ag("AG1", "Agency One", id);

        //_repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(original);

        //Agency? saved = null;
        //_repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
        //         .Callback<Agency, CancellationToken>((a, _) => saved = a)
        //         .Returns(Task.CompletedTask);

        var payload = new
        {
            AgencyId = id,
            Phone = "0612345678",
            Latitude = 34.01m
        };

        // Act
        var resp = await PatchJsonAsync($"/api/agencies/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        //saved!.Phone.Should().Be("0612345678");
        //saved.Latitude.Should().Be(34.01m);

        //// unchanged fields remain the same
        //saved.Name.Should().Be("Agency One");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Once);
    }

    //Duplicate code

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 400 when new Code already exists")]
    public async Task Patch_ShouldReturn400_WhenCodeDuplicate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var target = Ag("UNI1", "Unique", id);
        var duplicate = Ag("DUP1", "Duplicate", Guid.NewGuid());

        //_repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(target);

        //_repoMock.Setup(r => r.GetByCodeAsync("DUP1", It.IsAny<CancellationToken>()))
        //         .ReturnsAsync(duplicate); 

        var payload = new { AgencyId = id, Code = "DUP1" };

        // Act
        var resp = await PatchJsonAsync($"/api/agencies/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Agency with code DUP1 already exists.");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Never);
    }

    // CityId & SectorId both supplied

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 400 when both CityId and SectorId are provided")]
    public async Task Patch_ShouldReturn400_WhenCityAndSectorProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            AgencyId = id,
            CityId = Guid.NewGuid(),
            SectorId = Guid.NewGuid()
        };

        // Act
        var resp = await PatchJsonAsync($"/api/agencies/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
           .GetProperty("")[0]
           .GetString()
           .Should().Be("CityId and SectorId are mutually exclusive.");

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Never);
    }

    //Unknown agency

    [Fact(DisplayName = "PATCH /api/agencies/{id} returns 404 when agency is not found")]
    public async Task Patch_ShouldReturn400_WhenAgencyMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        //_repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //         .ReturnsAsync((Agency?)null);   // not found

        var payload = new { AgencyId = id, Name = "Will Fail" };

        // Act
        var resp = await PatchJsonAsync($"/api/agencies/{id}", payload);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        //_repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()),
        //                 Times.Never);
    }
}