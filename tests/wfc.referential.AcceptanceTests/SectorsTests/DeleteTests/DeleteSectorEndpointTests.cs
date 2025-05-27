using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.DeleteTests;

public class DeleteSectorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _sectorRepoMock = new();
    private readonly Mock<IAgencyRepository> _agencyRepoMock = new();

    public DeleteSectorEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<IAgencyRepository>();

                _sectorRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_sectorRepoMock.Object);
                services.AddSingleton(_agencyRepoMock.Object);
            });
        });
        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "US#46 - DELETE /api/sectors/{id} disables sector when no linked agencies")]
    public async Task DeleteSector_Should_DisableSector_WhenNoLinkedAgencies()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);
        _agencyRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agency>()); // No linked agencies

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{sectorId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify sector was disabled (soft delete)
        sector.IsEnabled.Should().BeFalse();

        _sectorRepoMock.Verify(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()), Times.Once);
        _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "US#46 - DELETE /api/sectors/{id} returns 400 when sector has linked agencies")]
    public async Task DeleteSector_Should_ReturnBadRequest_WhenSectorHasLinkedAgencies()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));

        var linkedAgency = Agency.Create(
            AgencyId.Of(Guid.NewGuid()),
            "AGY001", "Test Agency", "AGY", "Address", null, "Phone", "Fax",
            "Sheet", "Account", "MGRef", "MGPass", "12345", "Permission",
            null, null, null, SectorId.Of(sectorId), null, null, null);

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);
        _agencyRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agency> { linkedAgency });

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{sectorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify sector was not disabled
        sector.IsEnabled.Should().BeTrue();

        _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "US#46 - DELETE /api/sectors/{id} returns 400 when sector not found")]
    public async Task DeleteSector_Should_ReturnBadRequest_WhenSectorNotFound()
    {
        // Arrange
        var sectorId = Guid.NewGuid();

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector)null);

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{sectorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "US#46 - DELETE /api/sectors/{id} validates SectorId format")]
    public async Task DeleteSector_Should_ReturnBadRequest_WhenSectorIdInvalid()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _sectorRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<SectorId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "US#46 - DELETE /api/sectors/{id} performs soft delete not hard delete")]
    public async Task DeleteSector_Should_PerformSoftDelete_NotHardDelete()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);
        _agencyRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agency>());

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{sectorId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it's a soft delete - sector still exists but is disabled
        sector.IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "US#46 - DELETE /api/sectors/{id} checks all agency dependencies")]
    public async Task DeleteSector_Should_CheckAllAgencyDependencies_BeforeDeletion()
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{sectorId}");

        // Assert - should always check for linked agencies
        _agencyRepoMock.Verify(r => r.GetByConditionAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "US#46 - DELETE /api/sectors/{id} handles different agency scenarios")]
    [InlineData(0, true, "No agencies linked - should succeed")]
    [InlineData(1, false, "One agency linked - should fail")]
    [InlineData(5, false, "Multiple agencies linked - should fail")]
    public async Task DeleteSector_Should_HandleDifferentAgencyScenarios(int agencyCount, bool shouldSucceed, string scenario)
    {
        // Arrange
        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(
            SectorId.Of(sectorId),
            "SEC001", "Test Sector", CityId.Of(Guid.NewGuid()));

        var agencies = new List<Agency>();
        for (int i = 0; i < agencyCount; i++)
        {
            agencies.Add(Agency.Create(
                AgencyId.Of(Guid.NewGuid()),
                $"AGY{i:000}", $"Agency {i}", $"AG{i}", $"Address{i}", null, $"Phone{i}", $"Fax{i}",
                $"Sheet{i}", $"Account{i}", $"MGRef{i}", $"MGPass{i}", $"{12345 + i}", $"Permission{i}",
                null, null, null, SectorId.Of(sectorId), null, null, null));
        }

        _sectorRepoMock.Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == sectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);
        _agencyRepoMock.Setup(r => r.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Agency, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(agencies);

        // Act
        var response = await _client.DeleteAsync($"/api/sectors/{sectorId}");

        // Assert
        if (shouldSucceed)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK, because: scenario);
            sector.IsEnabled.Should().BeFalse();
            _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: scenario);
            sector.IsEnabled.Should().BeTrue(); // Should remain enabled
            _sectorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}