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
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Application.ParamTypes.Queries.GetFiltredParamTypes;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.GetFiltredTests;

public class GetFiltredParamTypesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repoMock = new();

    public GetFiltredParamTypesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IParamTypeRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    // Helper to build test ParamTypes
    private static ParamType CreateTestParamType(string value, bool isEnabled = true)
    {
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramType = ParamType.Create(
            new ParamTypeId(Guid.NewGuid()),
            typeDefinitionId,
            value
        );

        if (!isEnabled)
        {
            paramType.Disable();
        }

        return paramType;
    }

    [Fact(DisplayName = "GET /api/paramtypes returns all paramTypes using search criteria")]
    public async Task GetFiltredParamTypes_Should_ReturnAllParamTypes_UsingSearchCriteria()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramTypes = new List<ParamType>
        {
            CreateTestParamType("Value1"),
            CreateTestParamType("Value2"),
            CreateTestParamType("Value3")
        };

        var pagedResult = new PagedResult<ParamType>(paramTypes, paramTypes.Count, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetFiltredParamTypesQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?PageNumber=1&PageSize=10&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);

        // Verify all paramType data is returned
        result.Items.Should().Contain(p => p.Value == "Value1");
        result.Items.Should().Contain(p => p.Value == "Value2");
        result.Items.Should().Contain(p => p.Value == "Value3");
    }

    [Fact(DisplayName = "GET /api/paramtypes supports filtering by Value")]
    public async Task GetFiltredParamTypes_Should_FilterByValue_WhenValueProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var filteredParamTypes = new List<ParamType>
        {
            CreateTestParamType("Value1")
        };

        var pagedResult = new PagedResult<ParamType>(filteredParamTypes, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q => q.Value == "Value1"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?Value=Value1&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Value.Should().Be("Value1");
    }

    [Fact(DisplayName = "GET /api/paramtypes supports filtering by IsEnabled status")]
    public async Task GetFiltredParamTypes_Should_FilterByIsEnabled_WhenStatusProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var disabledParamTypes = new List<ParamType>
        {
            CreateTestParamType("DisabledValue", false)
        };

        var pagedResult = new PagedResult<ParamType>(disabledParamTypes, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q => q.IsEnabled == false), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?IsEnabled=false&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/paramtypes supports filtering by TypeDefinitionId")]
    public async Task GetFiltredParamTypes_Should_FilterByTypeDefinitionId_WhenTypeDefinitionProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramTypes = new List<ParamType>
        {
            CreateTestParamType("Value1"),
            CreateTestParamType("Value2")
        };

        var pagedResult = new PagedResult<ParamType>(paramTypes, 2, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q => q.TypeDefinitionId.Value == typeDefinitionId.Value),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.TypeDefinitionId.Value.Should().Be(typeDefinitionId.Value));
    }

    [Fact(DisplayName = "GET /api/paramtypes returns paginated results")]
    public async Task GetFiltredParamTypes_Should_ReturnPaginatedResults_WhenPaginationParametersProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramTypes = new List<ParamType>
        {
            CreateTestParamType("Value1"),
            CreateTestParamType("Value2")
        };

        var pagedResult = new PagedResult<ParamType>(paramTypes, 25, 2, 10); // Page 2 of 3 pages (25 total items)

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q => q.PageNumber == 2 && q.PageSize == 10),
            2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?PageNumber=2&PageSize=10&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GET /api/paramtypes uses default paging when no query params supplied")]
    public async Task GetFiltredParamTypes_Should_UseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        // We'll return 3 items – fewer than the default pageSize (10)
        var paramTypes = new List<ParamType>
        {
            CreateTestParamType("Value1"),
            CreateTestParamType("Value2"),
            CreateTestParamType("Value3")
        };

        var pagedResult = new PagedResult<ParamType>(paramTypes, paramTypes.Count, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
            1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
            1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/paramtypes returns empty result when no matches found")]
    public async Task GetFiltredParamTypes_Should_ReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var emptyResult = new PagedResult<ParamType>(new List<ParamType>(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetFiltredParamTypesQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?Value=NONEXISTENT&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/paramtypes supports multiple search criteria simultaneously")]
    public async Task GetFiltredParamTypes_Should_SupportMultipleCriteria_WhenMultipleFiltersProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var filteredParamTypes = new List<ParamType>
        {
            CreateTestParamType("TestValue")
        };

        var pagedResult = new PagedResult<ParamType>(filteredParamTypes, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.Is<GetFiltredParamTypesQuery>(q =>
                q.Value == "TestValue" &&
                q.IsEnabled == true &&
                q.TypeDefinitionId.Value == typeDefinitionId.Value),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act - Search with multiple criteria
        var response = await _client.GetAsync($"/api/paramtypes?Value=TestValue&IsEnabled=true&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var paramType = result.Items.First();
        paramType.Value.Should().Be("TestValue");
        paramType.IsEnabled.Should().BeTrue();
        paramType.TypeDefinitionId.Value.Should().Be(typeDefinitionId.Value);
    }

    [Theory(DisplayName = "GET /api/paramtypes supports various search criteria")]
    [InlineData("Value", "TestValue")]
    [InlineData("IsEnabled", "true")]
    [InlineData("IsEnabled", "false")]
    public async Task GetFiltredParamTypes_Should_SupportVariousSearchCriteria(string filterType, string filterValue)
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramTypes = new List<ParamType>
        {
            CreateTestParamType("TestValue", filterValue == "true" || filterType != "IsEnabled")
        };

        var pagedResult = new PagedResult<ParamType>(paramTypes, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetFiltredParamTypesQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?{filterType}={filterValue}&typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetFiltredParamTypesQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/paramtypes returns all paramType data in response")]
    public async Task GetFiltredParamTypes_Should_ReturnAllParamTypeData_InResponse()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramTypes = new List<ParamType>
        {
            CreateTestParamType("TestValue")
        };

        var pagedResult = new PagedResult<ParamType>(paramTypes, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetFiltredParamTypesQuery>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?typeDefinitionId={typeDefinitionId.Value}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParamTypesResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var paramType = result.Items.First();
        paramType.ParamTypeId.Should().NotBeEmpty();
        paramType.Value.Should().Be("TestValue");
        paramType.IsEnabled.Should().BeTrue();
        paramType.TypeDefinitionId.Should().NotBeNull();
    }
}