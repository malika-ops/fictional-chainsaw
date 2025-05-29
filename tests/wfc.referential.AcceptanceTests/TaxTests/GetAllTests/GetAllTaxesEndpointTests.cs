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
using wfc.referential.Application.Taxes.Queries.GetAllTaxes;
using wfc.referential.Domain.TaxAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.GetAllTests;

public class GetAllTaxesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRepository> _repoMock = new();
    private const string BaseUrl  = "api/taxes";

    public GetAllTaxesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static Tax DummyTax(string code, string description) =>
        Tax.Create(TaxId.Of(Guid.NewGuid()), code, $"{code}AR", $"{code}EN", description, 42, 20);
    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize,
                                     int TotalCount, int TotalPages);

    [Fact(DisplayName = $"GET {BaseUrl} returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allTaxs = new[] { 
            DummyTax("VAT_DE", "Germany VAT - Standard Rate"),
            DummyTax("GST_CA", "Canada GST - Federal Goods and Services Tax"),
            DummyTax("SALETX_CA", "US Sales Tax - California"),
            DummyTax("VAT_UK", "United Kingdom VAT - Standard Rate"),
            DummyTax("CST_IN", "India Central Sales Tax") 
        };

        // repository returns first 2 items for page=1 size=2
        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetAllTaxesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1,2,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<Tax>(allTaxs.Take(2).ToList(), 5, 1, 2));

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
    }

    // 2) Filter by code
    [Fact(DisplayName = $"GET {BaseUrl}?code=VAT_DE returns only VAT_DE tax")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var tax = DummyTax("VAT_DE", "Germany VAT - Standard Rate");

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetAllTaxesQuery>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PagedResult<Tax>(new List<Tax> { tax }, 2, 1, 2));


        // Act
        var response = await _client.GetAsync($"{BaseUrl}?code=VAT_DE");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("VAT_DE");

    }


}
