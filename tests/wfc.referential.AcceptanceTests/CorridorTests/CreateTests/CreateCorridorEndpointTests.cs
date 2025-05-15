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
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CorridorTests
{
    public class CreateCorridorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<ICorridorRepository> _repoMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private const string BaseUrl = "api/corridors";

        public CreateCorridorEndpointTests(WebApplicationFactory<Program> factory)
        {
            var customizedFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ICorridorRepository>();
                    services.RemoveAll<ICacheService>();

                    _repoMock
                        .Setup(r => r.AddCorridorAsync(It.IsAny<Corridor>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Corridor corridor, CancellationToken _) => corridor);

                    services.AddSingleton(_repoMock.Object);
                    services.AddSingleton(_cacheMock.Object);
                });
            });

            _client = customizedFactory.CreateClient();
        }

        [Fact(DisplayName = $"POST {BaseUrl} returns 200 and Guid when request is valid")]
        public async Task Post_ShouldReturn200_AndGuid_WhenRequestIsValid()
        {
            var payload = new CreateCorridorRequest
            {
                SourceCountryId = Guid.NewGuid(),
                DestinationCountryId = Guid.NewGuid(),
                SourceCityId = Guid.NewGuid(),
                DestinationCityId = Guid.NewGuid(),
                SourceAgencyId = Guid.NewGuid(),
                DestinationAgencyId = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync(BaseUrl, payload);
            var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedId.Should().NotBeEmpty();

            _repoMock.Verify(r => r.AddCorridorAsync(It.Is<Corridor>(c =>
                c.SourceCountryId.Value == payload.SourceCountryId &&
                c.DestinationCountryId.Value == payload.DestinationCountryId
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = $"POST {BaseUrl} returns 400 when SourceCountryId is missing")]
        public async Task Post_ShouldReturn400_WhenSourceCountryIdIsMissing()
        {
            var payload = new CreateCorridorRequest
            {
                // SourceCountryId missing (default Guid.Empty)
                DestinationCountryId = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync(BaseUrl, payload);
            var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errors = doc!.RootElement.GetProperty("errors");
            errors.GetProperty("sourceCountryId")[0].GetString().Should().Be("SourceCountryId is required");

            _repoMock.Verify(r => r.AddCorridorAsync(It.IsAny<Corridor>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = $"POST {BaseUrl} returns 400 when DestinationCountryId is missing")]
        public async Task Post_ShouldReturn400_WhenDestinationCountryIdIsMissing()
        {
            var payload = new CreateCorridorRequest
            {
                SourceCountryId = Guid.NewGuid()
                // DestinationCountryId missing (default Guid.Empty)
            };

            var response = await _client.PostAsJsonAsync(BaseUrl, payload);
            var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errors = doc!.RootElement.GetProperty("errors");
            errors.GetProperty("destinationCountryId")[0].GetString().Should().Be("DestinationCountryId is required");

            _repoMock.Verify(r => r.AddCorridorAsync(It.IsAny<Corridor>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
