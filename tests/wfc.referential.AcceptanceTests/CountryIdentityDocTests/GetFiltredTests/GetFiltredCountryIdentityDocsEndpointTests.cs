using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetFiltredCountryIdentityDocs;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.GetFiltredTests;

public class GetFiltredCountryIdentityDocsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static CountryIdentityDoc CreateTestCountryIdentityDoc(Guid countryId, Guid docId, bool isEnabled = true)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(Guid.NewGuid()),
            new Domain.Countries.CountryId(countryId),
            Domain.IdentityDocumentAggregate.IdentityDocumentId.Of(docId));
    }

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

        var pagedResult = new PagedResult<CountryIdentityDoc>(
            all.Take(2).ToList(),
            totalCount: 4,
            pageNumber: 1,
            pageSize: 2
        );

        _countryIdentityDocRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                        1, 2,
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

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

        _countryIdentityDocRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2,
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

        var pagedResult = new PagedResult<CountryIdentityDoc>(
            list.ToList(),
            totalCount: 2,
            pageNumber: 1,
            pageSize: 10
        );

        _countryIdentityDocRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                        1, 10,
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        // Act
        var response = await _client.GetAsync("/api/countryidentitydocs");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);

        _countryIdentityDocRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            1, 10,
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

        var pagedResult = new PagedResult<CountryIdentityDoc>(
            new List<CountryIdentityDoc> { association },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10
        );

        _countryIdentityDocRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.CountryId == countryId),
                        1, 10,
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        // Act
        var response = await _client.GetAsync($"/api/countryidentitydocs?countryId={countryId}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("countryId").GetGuid().Should().Be(countryId);

        _countryIdentityDocRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.CountryId == countryId),
                            1, 10,
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/countryidentitydocs?identityDocumentId={id} filters by identity document")]
    public async Task Get_ShouldFilterByIdentityDocumentId()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var association = CreateTestCountryIdentityDoc(countryId, docId);

        var pagedResult = new PagedResult<CountryIdentityDoc>(
            new List<CountryIdentityDoc> { association },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10
        );

        _countryIdentityDocRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.IdentityDocumentId == docId),
                        1, 10,
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        // Act
        var response = await _client.GetAsync($"/api/countryidentitydocs?identityDocumentId={docId}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("identityDocumentId").GetGuid().Should().Be(docId);

        _countryIdentityDocRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.IdentityDocumentId == docId),
                            1, 10,
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/countryidentitydocs?isEnabled=false filters by status")]
    public async Task Get_ShouldFilterByStatus()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var association = CreateTestCountryIdentityDoc(countryId, docId);
        association.Disable(); // Disable the association

        var pagedResult = new PagedResult<CountryIdentityDoc>(
            new List<CountryIdentityDoc> { association },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10
        );

        _countryIdentityDocRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.IsEnabled == false),
                        1, 10,
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        // Act
        var response = await _client.GetAsync("/api/countryidentitydocs?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _countryIdentityDocRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredCountryIdentityDocsQuery>(q => q.IsEnabled == false),
                            1, 10,
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }
}