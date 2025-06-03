using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Corridors.Queries.GetAllCorridors;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.CorridorTests
{
    public class GetAllCorridorsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<ICorridorRepository> _repoMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private const string BaseUrl = "api/corridors";

        public GetAllCorridorsEndpointTests(WebApplicationFactory<Program> factory)
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

        private static Corridor CreateDummyCorridor(Guid id, Guid sourceCountryId, Guid destinationCountryId)
        {
            return Corridor.Create(
                CorridorId.Of(id), 
                CountryId.Of(sourceCountryId),
                CountryId.Of(destinationCountryId),
                CityId.Of(Guid.NewGuid()),
                CityId.Of(Guid.NewGuid()),
                AgencyId.Of(Guid.NewGuid()),
                AgencyId.Of(Guid.NewGuid()));
        }

        private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

        [Fact(DisplayName = $"GET {BaseUrl} returns paged list")]
        public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
        {
            // Arrange
            var allCorridors = new[]
            {
                CreateDummyCorridor(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                CreateDummyCorridor(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                CreateDummyCorridor(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                CreateDummyCorridor(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                CreateDummyCorridor(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
            };

            _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllCorridorsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1,2,
                                It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PagedResult<Corridor>(
                            allCorridors.Take(2).ToList(),
                            5, 1, 2));


            // Act
            var response = await _client.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=2");
            var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            dto!.Items.Should().HaveCount(2);
            dto.TotalCount.Should().Be(5);
            dto.TotalPages.Should().Be(3);
            dto.PageNumber.Should().Be(1);
            dto.PageSize.Should().Be(2);

            _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                    It.Is<GetAllCorridorsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                    1, 2,
                                    It.IsAny<CancellationToken>()),
                             Times.Once);
        }

        [Fact(DisplayName = $"GET {BaseUrl}?sourceCountryId=xxx returns filtered list")]
        public async Task Get_ShouldFilterBySourceCountryId()
        {
            // Arrange
            var sourceCountryId = Guid.NewGuid();
            var filteredCorridor = CreateDummyCorridor(Guid.NewGuid(), sourceCountryId, Guid.NewGuid());

            _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetAllCorridorsQuery>(q => q.SourceCountryId != null && q.SourceCountryId.Value == sourceCountryId),
                                1, 10,
                                It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PagedResult<Corridor>(
                            new List<Corridor> { filteredCorridor },
                            5, 1, 2));

            // Act
            var response = await _client.GetAsync($"{BaseUrl}?sourceCountryId={sourceCountryId}");
            var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            dto!.Items.Should().HaveCount(1);
            dto.Items[0].GetProperty("sourceCountryId").GetGuid().Should().Be(sourceCountryId);

            _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                    It.Is<GetAllCorridorsQuery>(q => q.SourceCountryId != null && q.SourceCountryId.Value == sourceCountryId),
                                    1, 10,
                                    It.IsAny<CancellationToken>()),
                             Times.Once);
        }
    }
}
