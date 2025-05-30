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
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.DeleteTests;

public class DeleteAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();
    private readonly Mock<ICorridorRepository> _corRepoMock = new();

    public DeleteAgencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ICorridorRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(_corRepoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }


    private static Agency MakeAgency(Guid id, string code = "AG1") =>
        Agency.Create(
            AgencyId.Of(id),
            code,
            "Main Agency",
            "MA",
            "1 Main St",
            null,
            phone: "0600000000",
            fax: "",
            accountingSheetName: "Sheet",
            accountingAccountNumber: "401122",
            postalCode: "10000",
            latitude: null,
            longitude: null,
            cashTransporter: null,
            expenseFundAccountingSheet: null,
            expenseFundAccountNumber: null,
            madAccount: null,
            fundingThreshold: null,
            cityId: CityId.Of(Guid.NewGuid()),  
            sectorId: null,
            agencyTypeId: null,
            tokenUsageStatusId: null,
            fundingTypeId: null,
            partnerId: null,
            supportAccountId: null
        );

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }


    [Fact(DisplayName = "DELETE /api/agencies/{id} returns 200 when deletion succeeds")]
    public async Task Delete_ShouldReturn200_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agency = MakeAgency(id);

        _repoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(agency);

        // Act
        var resp = await _client.DeleteAsync($"/api/agencies/{id}");
        var result = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        agency.IsEnabled.Should().BeFalse();    // soft-deleted
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/agencies/{id} returns 400 when id is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenIdIsEmpty()
    {
        // Act
        var resp = await _client.DeleteAsync("/api/agencies/00000000-0000-0000-0000-000000000000");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "AgencyId")
            .Should().Be("AgencyId must be a non-empty GUID.");

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/agencies/{id} returns 400 when agency not found")]
    public async Task Delete_ShouldReturn404_WhenAgencyNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(AgencyId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Agency?)null);

        // Act
        var resp = await _client.DeleteAsync($"/api/agencies/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}