using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.CorridorTests
{
    public class PatchCorridorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<ICorridorRepository> _repoMock = new();
        private const string BaseUrl = "api/corridors";

        public PatchCorridorEndpointTests(WebApplicationFactory<Program> factory)
        {
            var customizedFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ICorridorRepository>();

                    services.AddSingleton(_repoMock.Object);
                });
            });

            _client = customizedFactory.CreateClient();
        }

        [Fact(DisplayName = $"PATCH {BaseUrl}/id updates corridor partially and returns updated id")]
        public async Task Patch_ShouldReturnUpdatedId_WhenCorridorExists()
        {
            // Arrange
            var corridorId = Guid.NewGuid();

            var existingCorridor = Corridor.Create(
                CorridorId.Of(corridorId),
                CountryId.Of(Guid.NewGuid()),
                CountryId.Of(Guid.NewGuid()),
                CityId.Of(Guid.NewGuid()),
                CityId.Of(Guid.NewGuid()),
                AgencyId.Of(Guid.NewGuid()),
                AgencyId.Of(Guid.NewGuid()));

            _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Corridor, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCorridor);

            _repoMock.Setup(r => r.Update(It.IsAny<Corridor>()));

            var patchRequest = new PatchCorridorRequest
            {
                CorridorId = corridorId,
                SourceCountryId = Guid.NewGuid(),
                IsEnabled = false
            };

            // Act
            var response = await _client.PatchAsync($"{BaseUrl}/{corridorId}", JsonContent.Create(patchRequest));
            var updatedId = await response.Content.ReadFromJsonAsync<bool>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedId.Should().Be(true);
        }

        [Fact(DisplayName = $"PATCH {BaseUrl}/id returns 404 when corridor does not exist")]
        public async Task Patch_ShouldReturn404_WhenCorridorDoesNotExist()
        {
            // Arrange
            var corridorId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Corridor, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Corridor)null);

            var patchRequest = new PatchCorridorRequest
            {
                CorridorId = corridorId,
                SourceCountryId = Guid.NewGuid()
            };

            // Act
            var response = await _client.PatchAsync($"{BaseUrl}/{corridorId}", JsonContent.Create(patchRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            _repoMock.Verify(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Corridor, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.Update(It.IsAny<Corridor>()), Times.Never);
        }

        [Fact(DisplayName = $"PATCH {BaseUrl}/id returns 400 when validation fails")]
        public async Task Patch_ShouldReturn400_WhenValidationFails()
        {
            // Arrange
            var corridorId = Guid.NewGuid();

            // Invalid SourceCountryId (empty Guid)
            var patchRequest = new PatchCorridorRequest
            {
                CorridorId = corridorId,
                SourceCountryId = Guid.Empty
            };

            // Act
            var response = await _client.PatchAsync($"{BaseUrl}/{corridorId}", JsonContent.Create(patchRequest));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _repoMock.Verify(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Corridor, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.Update(It.IsAny<Corridor>()), Times.Never);
        }
    }
}
