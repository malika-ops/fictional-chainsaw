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


namespace wfc.referential.AcceptanceTests.PartnerCountryTests.DeleteTests;

public class DeletePartnerCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerCountryRepository> _repoMock = new();

    public DeletePartnerCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var configured = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                /* replace infra with mocks */
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                                   It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = configured.CreateClient();
    }


    private static PartnerCountry Make(Guid id, Guid? partnerId = null, Guid? countryId = null) =>
        PartnerCountry.Create(
            PartnerCountryId.Of(id),
            new PartnerId(partnerId ?? Guid.NewGuid()),
            new CountryId(countryId ?? Guid.NewGuid()),
            true);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }


    [Fact(DisplayName = "DELETE /api/partnerCountries/{id} returns 200 when deletion succeeds")]
    public async Task Delete_ShouldReturn200_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var row = Make(id);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(row);

        PartnerCountry? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                           It.IsAny<CancellationToken>()))
                 .Callback<PartnerCountry, CancellationToken>((pc, _) => saved = pc);

        // Act
        var resp = await _client.DeleteAsync($"/api/partnerCountries/{id}");
        var success = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        success.Should().BeTrue();

        saved!.IsEnabled.Should().BeFalse();      // soft-deleted
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partnerCountries/{id} returns 400 when id is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenIdIsEmpty()
    {
        // Act
        var resp = await _client.DeleteAsync("/api/partnerCountries/00000000-0000-0000-0000-000000000000");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "PartnerCountryId")
            .Should().Be("PartnerCountryId must be a non-empty GUID.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partnerCountries/{id} returns 400 when PartnerCountry not found")]
    public async Task Delete_ShouldReturn400_WhenPartnerCountryNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((PartnerCountry?)null);

        // Act
        var resp = await _client.DeleteAsync($"/api/partnerCountries/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
            .Should().Be($"PartnerCountry [{id}] not found.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<PartnerCountry>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
