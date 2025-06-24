using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Products.Queries.GetFiltredProducts;
using wfc.referential.Domain.ProductAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ProductTests.GetFiltredTests;

public class GetFiltredProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IProductRepository> _repoMock = new();

    public GetFiltredProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IProductRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy Products quickly
    private static Product DummyProduct(string code, string name) =>
        Product.Create(ProductId.Of(Guid.NewGuid()), code, name, true);

    // Lightweight DTO for deserialising the endpoint response
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    // 1) Happy‑path paging
    [Fact(DisplayName = "GET /api/products returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allProducts = new[] { DummyProduct("001", "CashExpress"), DummyProduct("002", "TransfertNational"),
                               DummyProduct("002", "Floussy"),    DummyProduct("004", "TransfertInternational"),
                               DummyProduct("005", "Jibi") };

        // repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredProductsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<Product>( allProducts.Take(2).ToList(), 5,1,2));

        // Act
        var response = await _client.GetAsync("/api/products?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredProductsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1, 2,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Filter by code
    [Fact(DisplayName = "GET /api/products?code=001 returns only CashExpress Product")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var usd = DummyProduct("001", "Cash Express");


        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.IsAny<GetFiltredProductsQuery>(),
                    1, 10,
                    It.IsAny<CancellationToken>()))
         .ReturnsAsync(new PagedResult<Product>(new List<Product> { usd }, 1, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/products?code=001");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("001");

    }

    //// 3) Default paging when parameters are omitted
    [Fact(DisplayName = "GET /api/products uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        // Arrange
        // we’ll return 3 items – fewer than the default pageSize (10)
        var zones = new[] { DummyProduct("001", "Cash Express"),
                        DummyProduct("002", "Floussy"),
                        DummyProduct("003", "Jibi") };

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.IsAny<GetFiltredProductsQuery>(),
                    1, 10,
                    It.IsAny<CancellationToken>()))
         .ReturnsAsync(new PagedResult<Product>(zones.ToList(), 5, 1, 10));

        // Act
        var response = await _client.GetAsync("/api/products");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        // repository must have been called with default paging values
        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredProductsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                                1, 10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

}
