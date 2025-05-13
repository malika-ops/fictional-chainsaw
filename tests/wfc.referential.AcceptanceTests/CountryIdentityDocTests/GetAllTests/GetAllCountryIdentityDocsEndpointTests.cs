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
using wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.GetAllTests;

public class GetAllCountryIdentityDocsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryIdentityDocRepository> _repoMock = new();
    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<IIdentityDocumentRepository> _identityDocRepoMock = new();

    public GetAllCountryIdentityDocsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryIdentityDocRepository>();
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<IIdentityDocumentRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_identityDocRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper pour créer rapidement un CountryIdentityDoc
    private static CountryIdentityDoc CreateTestCountryIdentityDoc(Guid countryId, Guid docId, bool isEnabled = true)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(Guid.NewGuid()),
            new CountryId(countryId),
            IdentityDocumentId.Of(docId),
            isEnabled
        );
    }

    // DTO pour désérialiser la réponse paginée
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/countryidentitydocs returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var countryId1 = Guid.NewGuid();
        var countryId2 = Guid.NewGuid();
        var docId1 = Guid.NewGuid();
        var docId2 = Guid.NewGuid();

        var all = new[] {
            CreateTestCountryIdentityDoc(countryId1, docId1),
            CreateTestCountryIdentityDoc(countryId1, docId2),
            CreateTestCountryIdentityDoc(countryId2, docId1),
            CreateTestCountryIdentityDoc(countryId2, docId2)
        };

        var country1 = Country.Create(
            new CountryId(countryId1),
            "FR",
            "France",
            "FRA",
            "FR",
            "FRA",
            "+33",
            "GMT+1",
            false,
            false,
            2,
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        var doc1 = IdentityDocument.Create(
            IdentityDocumentId.Of(docId1),
            "CIN",
            "Carte d'identité",
            "Description",
            true
        );

        _repoMock.Setup(r => r.GetFilteredAsync(
                        It.Is<GetAllCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(all.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                        It.IsAny<GetAllCountryIdentityDocsQuery>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(all.Length);

        // Update to use GetAllCountriesAsync instead of GetAllCountriesQueryable
        _countryRepoMock.Setup(r => r.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Country> { country1 });

        _identityDocRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IdentityDocument> { doc1 });

        // Act
        var response = await _client.GetAsync("/api/countryidentitydocs?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(4);
        dto.TotalPages.Should().Be(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredAsync(
                            It.Is<GetAllCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/countryidentitydocs?countryId={id} filters by country")]
    public async Task Get_ShouldFilterByCountryId()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var association = CreateTestCountryIdentityDoc(countryId, docId);

        var country = Country.Create(
            new CountryId(countryId),
            "FR",
            "France",
            "FRA",
            "FR",
            "FRA",
            "+33",
            "GMT+1",
            false,
            false,
            2,
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description",
            true
        );

        _repoMock.Setup(r => r.GetFilteredAsync(
                        It.Is<GetAllCountryIdentityDocsQuery>(q => q.CountryId == countryId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryIdentityDoc> { association });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                        It.IsAny<GetAllCountryIdentityDocsQuery>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Update to use GetAllCountriesAsync instead of GetAllCountriesQueryable
        _countryRepoMock.Setup(r => r.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Country> { country });

        _identityDocRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IdentityDocument> { doc });

        // Act
        var response = await _client.GetAsync($"/api/countryidentitydocs?countryId={countryId}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("countryId").GetGuid().Should().Be(countryId);

        _repoMock.Verify(r => r.GetFilteredAsync(
                            It.Is<GetAllCountryIdentityDocsQuery>(q => q.CountryId == countryId),
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/countryidentitydocs uses default paging")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var list = new[] {
            CreateTestCountryIdentityDoc(countryId, docId),
            CreateTestCountryIdentityDoc(Guid.NewGuid(), Guid.NewGuid())
        };

        _repoMock.Setup(r => r.GetFilteredAsync(
                        It.Is<GetAllCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(list.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                        It.IsAny<GetAllCountryIdentityDocsQuery>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(list.Length);

        // Update to use GetAllCountriesAsync instead of GetAllCountriesQueryable
        _countryRepoMock.Setup(r => r.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Country>());

        _identityDocRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IdentityDocument>());

        // Act
        var response = await _client.GetAsync("/api/countryidentitydocs");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _repoMock.Verify(r => r.GetFilteredAsync(
                            It.Is<GetAllCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }
}