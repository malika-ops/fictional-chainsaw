using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.CorridorTests
{
    public class DeleteCorridorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<ICorridorRepository> _repoMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private const string BaseUrl = "api/corridors";

        public DeleteCorridorEndpointTests(WebApplicationFactory<Program> factory)
        {
            var customizedFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ICorridorRepository>();
                    services.RemoveAll<ICacheService>();

                    services.AddSingleton(_repoMock.Object);
                    services.AddSingleton(_cacheMock.Object);
                });
            });

            _client = customizedFactory.CreateClient();
        }

        [Fact(DisplayName = $"DELETE {BaseUrl}/id returns true when corridor is deleted successfully")]
        public async Task Delete_ShouldReturnTrue_WhenCorridorExists()
        {
            // Arrange
            var corridorId = Guid.NewGuid();

            var corridor = Corridor.Create(
                CorridorId.Of(corridorId),
                CountryId.Of(Guid.NewGuid()),
                CountryId.Of(Guid.NewGuid()),
                CityId.Of(Guid.NewGuid()),
                CityId.Of(Guid.NewGuid()),
                AgencyId.Of(Guid.NewGuid()),
                AgencyId.Of(Guid.NewGuid()));

            _repoMock.Setup(r => r.GetByIdAsync(corridorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(corridor);

            _repoMock.Setup(r => r.UpdateCorridorAsync(It.IsAny<Corridor>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _client.DeleteAsync($"{BaseUrl}/{corridorId}");
            var result = await response.Content.ReadFromJsonAsync<bool>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().BeTrue();

            _repoMock.Verify(r => r.GetByIdAsync(corridorId, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateCorridorAsync(It.Is<Corridor>(c => c.Id == CorridorId.Of(corridorId) && c.IsEnabled == false), It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(key => key.Contains(corridorId.ToString())), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = $"DELETE {BaseUrl}/id returns 404 when corridor does not exist")]
        public async Task Delete_ShouldReturn404_WhenCorridorDoesNotExist()
        {
            // Arrange
            var corridorId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(corridorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Corridor)null);

            // Act
            var response = await _client.DeleteAsync($"{BaseUrl}/{corridorId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            _repoMock.Verify(r => r.GetByIdAsync(corridorId, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateCorridorAsync(It.IsAny<Corridor>(), It.IsAny<CancellationToken>()), Times.Never);
            _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
