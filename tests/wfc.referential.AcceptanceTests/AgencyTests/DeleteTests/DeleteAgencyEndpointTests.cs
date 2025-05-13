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
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTests.DeleteTests;

public class DeleteAgencyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyRepository> _repoMock = new();

    public DeleteAgencyEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IAgencyRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agency>(),
                                                   It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static Agency MakeAgency(Guid id, string code = "AG1") =>
        Agency.Create(
            AgencyId.Of(id),
            code,
            name: "Main Agency",
            abbreviation: "MA",
            address1: "1 Main St",
            address2: null,
            phone: "123",
            fax: "456",
            accountingSheetName: "sheet",
            accountingAccountNumber: "acc-001",
            moneyGramReferenceNumber: "mg-ref",
            moneyGramPassword: "pwd",
            postalCode: "1000",
            permissionOfficeChange: "perm",
            latitude: null,
            longitude: null,
            isEnabled: true,
            cityId: null,
            sectorId: null,
            agencyTypeId: null,
            supportAccountId: null,
            partnerId: null);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------- tests ---------- */

    [Fact(DisplayName = "DELETE /api/agencies/{id} returns 200 when deletion succeeds")]
    public async Task Delete_ShouldReturn200_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agency = MakeAgency(id);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(agency);

        Agency? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agency>(),
                                           It.IsAny<CancellationToken>()))
                 .Callback<Agency, CancellationToken>((a, _) => saved = a)
                 .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/agencies/{id}");
        var success = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        success.Should().BeTrue();

        saved!.IsEnabled.Should().Be(false);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/agencies/{id} returns 400 when id is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenIdIsEmpty()
    {
        // Act
        var response = await _client.DeleteAsync("/api/agencies/00000000-0000-0000-0000-000000000000");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "AgencyId")
            .Should().Be("AgencyId must be a non-empty GUID.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/agencies/{id} returns 400 when agency not found")]
    public async Task Delete_ShouldReturn400_WhenAgencyNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Agency?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/agencies/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var msg = doc!.RootElement.GetProperty("errors").GetString();
        msg.Should().Be($"Agency [{id}] not found.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Agency>(),
                                            It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}