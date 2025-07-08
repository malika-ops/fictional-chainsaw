using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.TaxRuleDetails.Queries.GetFiltredTaxeRuleDetails;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class GetFiltredTaxRuleDetailsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private const string BaseUrl = "api/tax-rule-details";
    private static TaxRuleDetail CreateDummyTaxRuleDetail(Guid id)
    {
        return TaxRuleDetail.Create(
            TaxRuleDetailsId.Of(id),
            corridorId: CorridorId.Of(Guid.NewGuid()),
            taxId: TaxId.Of(Guid.NewGuid()),
            serviceId: ServiceId.Of(Guid.NewGuid()),
            appliedOn: ApplicationRule.Fees);
    }

    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

    [Fact(DisplayName = $"GET {BaseUrl} returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allTaxRuleDetails = new[]
        {
            CreateDummyTaxRuleDetail(Guid.NewGuid()),
            CreateDummyTaxRuleDetail(Guid.NewGuid()),
            CreateDummyTaxRuleDetail(Guid.NewGuid()),
            CreateDummyTaxRuleDetail(Guid.NewGuid()),
            CreateDummyTaxRuleDetail(Guid.NewGuid())
        };

        // Repository returns first 2 items for page=1 size=2
        _taxRuleDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<GetFiltredTaxRuleDetailsQuery>(),
                            It.IsAny<int>(), It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<TaxRuleDetail>(allTaxRuleDetails.Take(2).ToList(), 5, 1, 2));

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

        _taxRuleDetailsRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.Is<GetFiltredTaxRuleDetailsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                1, 2,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = $"GET {BaseUrl}?appliedOn=Fees returns filtered list")]
    public async Task Get_ShouldFilterByAppliedOn()
    {
        // Arrange
        var filteredTaxRuleDetail = CreateDummyTaxRuleDetail(Guid.NewGuid());

        _taxRuleDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredTaxRuleDetailsQuery>(q => q.AppliedOn == ApplicationRule.Fees),
                            1, 10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<TaxRuleDetail>( new List<TaxRuleDetail> { filteredTaxRuleDetail },1,1,10 ));

        _cacheMock.Setup(c => c.GetAsync<PagedResult<GetTaxRuleDetailsResponse>>(
                                It.IsAny<string>(),
                                It.IsAny<CancellationToken>()))
                  .ReturnsAsync((PagedResult<GetTaxRuleDetailsResponse>)null);

        _cacheMock.Setup(c => c.SetAsync(
                                It.IsAny<string>(),
                                It.IsAny<object>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        // Act
        var response = await _client.GetAsync($"{BaseUrl}?appliedOn=Fees");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("appliedOn").GetString().Should().Be("Fees");
    }
}
