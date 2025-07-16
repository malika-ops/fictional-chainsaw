using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Services.Queries.GetFiltredServices;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ServiceTests.GetFiltredTests;

public class GetFiltredServiceEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Service DummyService(string code, string name , FlowDirection flowDirection) =>
        Service.Create(ServiceId.Of(Guid.NewGuid()), code, name, flowDirection, true, ProductId.Of(Guid.NewGuid()));

    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/services returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        var allServices = new[] {
            DummyService("SVC001", "ExpressService",FlowDirection.Debit),
            DummyService("SVC002", "Floussy",FlowDirection.Debit),
            DummyService("SVC003", "Jibi", FlowDirection.Debit)
        };

        _serviceRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredServicesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<Service>(allServices.Take(2).ToList(), 3, 1, 2));

        var response = await _client.GetAsync("/api/services?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(3);
        dto.TotalPages.Should().Be(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);
    }

    [Fact(DisplayName = "GET /api/services?code=SVC001 returns only ExpressService")]
    public async Task Get_ShouldFilterByCode()
    {
        var svc = DummyService("SVC001", "ExpressService", FlowDirection.Debit);

        _serviceRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredServicesQuery>(q => q.Code == "SVC001"),
                            1, 10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<Service>(new List<Service> { svc }, 5, 1, 2));

        var response = await _client.GetAsync("/api/services?code=SVC001");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("SVC001");
    }

    [Fact(DisplayName = "GET /api/services uses default paging when no query params supplied")]
    public async Task Get_ShouldUseDefaultPaging_WhenNoParamsProvided()
    {
        var services = new[] {
            DummyService("SVC001", "Express",FlowDirection.Debit),
            DummyService("SVC002", "Floussy",FlowDirection.Debit),
            DummyService("SVC003", "Jibi",FlowDirection.Debit)
        };

        _serviceRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredServicesQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                            1, 10,
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new PagedResult<Service>(services.Take(2).ToList(), 3, 1, 2));


        var response = await _client.GetAsync("/api/services");
        var dto = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(2);
    }
}
