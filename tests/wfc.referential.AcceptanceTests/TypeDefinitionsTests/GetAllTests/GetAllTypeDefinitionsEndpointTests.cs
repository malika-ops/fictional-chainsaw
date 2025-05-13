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
using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.GetAllTests;

public class GetAllTypeDefinitionsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITypeDefinitionRepository> _repoMock = new();

    public GetAllTypeDefinitionsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITypeDefinitionRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

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

    [Fact(DisplayName = "GET /api/typedefinitions returns paged list")]
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

        // Repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetFilteredTypeDefinitionsAsync(
                            It.Is<GetAllTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(allTypeDefinitions.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTypeDefinitionsQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(allTypeDefinitions.Length);

        // Act
        var response = await _client.GetAsync("/api/typedefinitions?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetFilteredTypeDefinitionsAsync(
                                It.Is<GetAllTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/typedefinitions?libelle=Type1 returns only matching typeDefinition")]
    public async Task Get_ShouldFilterByLibelle()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("Type1", "Description 1");

        _repoMock.Setup(r => r.GetFilteredTypeDefinitionsAsync(
                            It.Is<GetAllTypeDefinitionsQuery>(q => q.Libelle == "Type1"),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TypeDefinition> { typeDefinition });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTypeDefinitionsQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/typedefinitions?libelle=Type1");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("libelle").GetString().Should().Be("Type1");

        _repoMock.Verify(r => r.GetFilteredTypeDefinitionsAsync(
                                It.Is<GetAllTypeDefinitionsQuery>(q => q.Libelle == "Type1"),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/typedefinitions?description=Description 2 returns only matching typeDefinition")]
    public async Task Get_ShouldFilterByDescription()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("Type2", "Description 2");

        _repoMock.Setup(r => r.GetFilteredTypeDefinitionsAsync(
                            It.Is<GetAllTypeDefinitionsQuery>(q => q.Description == "Description 2"),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TypeDefinition> { typeDefinition });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTypeDefinitionsQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/typedefinitions?description=Description 2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("description").GetString().Should().Be("Description 2");

        _repoMock.Verify(r => r.GetFilteredTypeDefinitionsAsync(
                                It.Is<GetAllTypeDefinitionsQuery>(q => q.Description == "Description 2"),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/typedefinitions uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // We'll return 3 items – fewer than the default pageSize (10)
        var typeDefinitions = new[] {
            CreateTestTypeDefinition("Type1", "Description 1"),
            CreateTestTypeDefinition("Type2", "Description 2"),
            CreateTestTypeDefinition("Type3", "Description 3")
        };

        _repoMock.Setup(r => r.GetFilteredTypeDefinitionsAsync(
                            It.Is<GetAllTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(typeDefinitions.ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTypeDefinitionsQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(typeDefinitions.Length);

        // Act
        var response = await _client.GetAsync("/api/typedefinitions");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // Repository must have been called with default paging values
        _repoMock.Verify(r => r.GetFilteredTypeDefinitionsAsync(
                                It.Is<GetAllTypeDefinitionsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "GET /api/typedefinitions?isEnabled=false returns only disabled typeDefinitions")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var typeDefinition = CreateTestTypeDefinition("Type1", "Description 1");
        typeDefinition.Disable(); // Make it disabled for the test

        _repoMock.Setup(r => r.GetFilteredTypeDefinitionsAsync(
                            It.Is<GetAllTypeDefinitionsQuery>(q => q.IsEnabled == false),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TypeDefinition> { typeDefinition });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTypeDefinitionsQuery>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        // Act
        var response = await _client.GetAsync("/api/typedefinitions?isEnabled=false");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();

        _repoMock.Verify(r => r.GetFilteredTypeDefinitionsAsync(
                                It.Is<GetAllTypeDefinitionsQuery>(q => q.IsEnabled == false),
                                It.IsAny<CancellationToken>()),
                        Times.Once);
    }
}