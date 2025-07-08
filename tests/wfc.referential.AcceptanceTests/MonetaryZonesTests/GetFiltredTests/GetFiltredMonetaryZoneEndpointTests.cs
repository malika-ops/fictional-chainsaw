using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.MonetaryZones.Queries.GetFiltredMonetaryZones;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.GetFiltredTests;

public class GetFiltredMonetaryZoneEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static MonetaryZone MakeZone(string code, bool enabled = true)
    {
        return MonetaryZone.Create(
            MonetaryZoneId.Of(Guid.NewGuid()),
            code,
            name: $"Zone-{code}",
            description: $"Description for {code}"
        ).Tap(z => { if (!enabled) z.Disable(); });
    }

    private record PagedResultDto<T>(
        T[] Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages);


    [Fact(DisplayName = "GET /api/monetaryZones returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsValid()
    {
        // Arrange
        var zones = new[]
        {
                MakeZone("EU"),
                MakeZone("US")
            };

        var paged = new PagedResult<MonetaryZone>(zones.ToList(), totalCount: 5, 1, 2);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            1, 2, It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        // Act
        var res = await _client.GetAsync("/api/monetaryZones?pageNumber=1&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);
    }

    [Fact(DisplayName = "GET /api/monetaryZones?code=EU filters by Code")]
    public async Task Get_ShouldFilterByCode()
    {
        const string code = "EU";
        var zone = MakeZone(code);
        var paged = new PagedResult<MonetaryZone>(new List<MonetaryZone> { zone }, 1, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.Code == code),
                            1, 10, It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/monetaryZones?code={code}");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be(code);
    }

    [Fact(DisplayName = "GET /api/monetaryZones?name=Europe filters by Name")]
    public async Task Get_ShouldFilterByName()
    {
        const string name = "Europe";
        var zone = MakeZone("EU");
        zone.Update(zone.Code, name, zone.Description, zone.IsEnabled);

        var paged = new PagedResult<MonetaryZone>(new List<MonetaryZone> { zone }, 1, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.Name == name),
                            1, 10, It.IsAny<CancellationToken>(), 
                            It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/monetaryZones?name={name}");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("name").GetString().Should().Be(name);
    }

    [Fact(DisplayName = "GET /api/monetaryZones?description=Desc filters by Description")]
    public async Task Get_ShouldFilterByDescription()
    {
        const string desc = "Special";
        var zone = MakeZone("SP");
        zone.Update(zone.Code, zone.Name, desc, zone.IsEnabled);

        var paged = new PagedResult<MonetaryZone>(new List<MonetaryZone> { zone }, 1, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.Description == desc),
                            1, 10, It.IsAny<CancellationToken>(), 
                            It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/monetaryZones?description={desc}");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("description").GetString().Should().Be(desc);
    }

    [Fact(DisplayName = "GET /api/monetaryZones?isEnabled=false filters disabled zones")]
    public async Task Get_ShouldFilterByEnabledFalse()
    {
        var disabled = MakeZone("DIS", enabled: false);
        var paged = new PagedResult<MonetaryZone>(new List<MonetaryZone> { disabled }, 1, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.IsEnabled == false),
                            1, 10, It.IsAny<CancellationToken>(), 
                            It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/monetaryZones?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/monetaryZones uses defaults when no params")]
    public async Task Get_ShouldUseDefaults_WhenNoParams()
    {
        var zones = new[]
        {
                MakeZone("A"), MakeZone("B"), MakeZone("C")
            };
        var paged = new PagedResult<MonetaryZone>(zones.ToList(), 3, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/monetaryZones");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "GET /api/monetaryZones with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        const string code = "MX";
        const string name = "Mexico";
        var zone = MakeZone(code);
        zone.Update(code, name, zone.Description, true);

        var paged = new PagedResult<MonetaryZone>(new List<MonetaryZone> { zone }, 1, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q =>
                                q.Code == code && q.Name == name && q.IsEnabled == true),
                            1, 10, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync($"/api/monetaryZones?code={code}&name={name}&isEnabled=true");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be(code);
    }

    [Fact(DisplayName = "GET /api/monetaryZones returns empty list when no match")]
    public async Task Get_ShouldReturnEmpty_WhenNoMatch()
    {
        var paged = new PagedResult<MonetaryZone>(new List<MonetaryZone>(), 0, 1, 10);

        _monetaryZoneRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.Is<GetFiltredMonetaryZonesQuery>(q => q.Code == "NONE"),
                            1, 10, It.IsAny<CancellationToken>(), 
                            It.IsAny<Expression<Func<MonetaryZone, object>>[]>()))
                 .ReturnsAsync(paged);

        var res = await _client.GetAsync("/api/monetaryZones?code=NONE");
        var dto = await res.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}

internal static class TapExt
{
    internal static T Tap<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}