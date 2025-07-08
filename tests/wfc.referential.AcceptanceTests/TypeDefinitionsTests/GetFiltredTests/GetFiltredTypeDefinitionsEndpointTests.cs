using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.TypeDefinitions.Queries.GetFiltredTypeDefinitions;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.GetFiltredTests;

public class GetFiltredTypeDefinitionsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    // Helper to build test TypeDefinitions
    private static TypeDefinition CreateTestTypeDefinition(string libelle, string description)
    {
        return TypeDefinition.Create(
            new TypeDefinitionId(Guid.NewGuid()),
            libelle,
            description
        );
    }

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                    int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/type-definitions returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allTypeDefinitions = new[] {
            CreateTestTypeDefinition("Type1", "Description 1"),
            CreateTestTypeDefinition("Type2", "Description 2"),
            CreateTestTypeDefinition("Type3", "Description 3"),
            CreateTestTypeDefinition("Type4", "Description 4"),
            CreateTestTypeDefinition("Type5", "Description 5")
        };

        // Repository returns paged result for page=1 size=2
        var pagedResult = new PagedResult<TypeDefinition>(
            totalCount: allTypeDefinitions.Length,
            items: allTypeDefinitions.Take(2).ToList(),
            pageNumber: 1,
            pageSize: 2
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2,
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/type-definitions?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _typeDefinitionRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1, 2,
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/type-definitions?libelle=Type1 returns only matching typeDefinition")]
    public async Task Get_ShouldFilterByLibelle()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("Type1", "Description 1");

        var pagedResult = new PagedResult<TypeDefinition>(
            totalCount: 1,
            items: new List<TypeDefinition> { typeDefinition },
            pageNumber: 1,
            pageSize: 10
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTypeDefinitionsQuery>(q => q.Libelle == "Type1"),
                            It.IsAny<int>(), It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/type-definitions?libelle=Type1");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("libelle").GetString().Should().Be("Type1");

        _typeDefinitionRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredTypeDefinitionsQuery>(q => q.Libelle == "Type1"),
                                It.IsAny<int>(), It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/type-definitions?description=Description 2 returns only matching typeDefinition")]
    public async Task Get_ShouldFilterByDescription()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("Type2", "Description 2");

        var pagedResult = new PagedResult<TypeDefinition>(
            totalCount: 1,
            items: new List<TypeDefinition> { typeDefinition },
            pageNumber: 1,
            pageSize: 10
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTypeDefinitionsQuery>(q => q.Description == "Description 2"),
                            It.IsAny<int>(), It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/type-definitions?description=Description 2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("description").GetString().Should().Be("Description 2");

        _typeDefinitionRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredTypeDefinitionsQuery>(q => q.Description == "Description 2"),
                                It.IsAny<int>(), It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/type-definitions uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // We'll return 3 items – fewer than the default pageSize (10)
        var typeDefinitions = new[] {
            CreateTestTypeDefinition("Type1", "Description 1"),
            CreateTestTypeDefinition("Type2", "Description 2"),
            CreateTestTypeDefinition("Type3", "Description 3")
        };

        var pagedResult = new PagedResult<TypeDefinition>(
            totalCount: typeDefinitions.Length,
            items: typeDefinitions.ToList(),
            pageNumber: 1,
            pageSize: 10
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            1, 10,
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/type-definitions");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _typeDefinitionRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                1, 10,
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/type-definitions?isEnabled=false returns only disabled typeDefinitions")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("Type1", "Description 1");
        typeDefinition.Disable(); // Make it disabled for the test

        var pagedResult = new PagedResult<TypeDefinition>(
            totalCount: 1,
            items: new List<TypeDefinition> { typeDefinition },
            pageNumber: 1,
            pageSize: 10
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTypeDefinitionsQuery>(q => q.IsEnabled == false),
                            It.IsAny<int>(), It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/type-definitions?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _typeDefinitionRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredTypeDefinitionsQuery>(q => q.IsEnabled == false),
                                It.IsAny<int>(), It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/type-definitions returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<TypeDefinition>(
            totalCount: 0,
            items: new List<TypeDefinition>(),
            pageNumber: 1,
            pageSize: 10
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<GetFiltredTypeDefinitionsQuery>(),
                            It.IsAny<int>(), It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/type-definitions?libelle=NONEXISTENT");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/type-definitions supports multiple search criteria simultaneously")]
    public async Task Get_ShouldSupportMultipleCriteria_WhenMultipleFiltersProvided()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("TestType", "TestDescription");

        var pagedResult = new PagedResult<TypeDefinition>(
            totalCount: 1,
            items: new List<TypeDefinition> { typeDefinition },
            pageNumber: 1,
            pageSize: 10
        );

        _typeDefinitionRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTypeDefinitionsQuery>(q =>
                                q.Libelle == "TestType" &&
                                q.Description == "TestDescription" &&
                                q.IsEnabled == true),
                            It.IsAny<int>(), It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

        // Act - Search with multiple criteria
        var response = await _client.GetAsync("/api/type-definitions?libelle=TestType&description=TestDescription&isEnabled=true");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);

        var item = dto.Items.First();
        item.GetProperty("libelle").GetString().Should().Be("TestType");
        item.GetProperty("description").GetString().Should().Be("TestDescription");
        item.GetProperty("isEnabled").GetBoolean().Should().BeTrue();
    }
}
