using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Taxes.Queries.GetFiltredTaxes;
using wfc.referential.Domain.TaxAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.GetFiltredTests;

public class GetFiltredTaxesEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl  = "api/taxes";

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
        _taxRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTaxesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
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

        _taxRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
            It.IsAny<GetFiltredTaxesQuery>(),
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
