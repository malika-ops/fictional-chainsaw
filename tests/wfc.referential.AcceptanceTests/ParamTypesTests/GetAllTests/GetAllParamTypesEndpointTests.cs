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
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.GetAllTests;

public class GetAllParamTypesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repoMock = new();

    public GetAllParamTypesEndpointTests(WebApplicationFactory<Program> factory)
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

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                    int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/paramtypes returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var allParamTypes = new[] {
            CreateTestParamType("Value1"),
            CreateTestParamType("Value2"),
            CreateTestParamType("Value3"),
            CreateTestParamType("Value4"),
            CreateTestParamType("Value5")
        };

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetFilteredParamTypesAsync(
                            It.Is<GetAllParamTypesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(allParamTypes.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllParamTypesQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(allParamTypes.Length);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?pageNumber=1&pageSize=2&typeDefinitionId={typeDefinitionId.Value}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredParamTypesAsync(
                                It.Is<GetAllParamTypesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/paramtypes?value=Value1 returns only matching paramType")]
    public async Task Get_ShouldFilterByValue()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var paramType = CreateTestParamType("Value1");

        _repoMock.Setup(r => r.GetFilteredParamTypesAsync(
                            It.Is<GetAllParamTypesQuery>(q => q.Value == "Value1"),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ParamType> { paramType });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllParamTypesQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?value=Value1&typeDefinitionId={typeDefinitionId.Value}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("value").GetString().Should().Be("Value1");

        _repoMock.Verify(r => r.GetFilteredParamTypesAsync(
                                It.Is<GetAllParamTypesQuery>(q => q.Value == "Value1"),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/paramtypes?isEnabled=false returns only disabled paramTypes")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var disabledParamType = CreateTestParamType("DisabledValue", false);

        _repoMock.Setup(r => r.GetFilteredParamTypesAsync(
                            It.Is<GetAllParamTypesQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ParamType> { disabledParamType });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllParamTypesQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?isEnabled=false&typeDefinitionId={typeDefinitionId.Value}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredParamTypesAsync(
                                It.Is<GetAllParamTypesQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/paramtypes uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        var typeDefinitionId = TypeDefinitionId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        // We'll return 3 items – fewer than the default pageSize (10)
        var paramTypes = new[] {
            CreateTestParamType("Value1"),
            CreateTestParamType("Value2"),
            CreateTestParamType("Value3")
        };

        _repoMock.Setup(r => r.GetFilteredParamTypesAsync(
                            It.Is<GetAllParamTypesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(paramTypes.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllParamTypesQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(paramTypes.Length);

        // Act
        var response = await _client.GetAsync($"/api/paramtypes?typeDefinitionId={typeDefinitionId.Value}");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _repoMock.Verify(r => r.GetFilteredParamTypesAsync(
                                It.Is<GetAllParamTypesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }
}