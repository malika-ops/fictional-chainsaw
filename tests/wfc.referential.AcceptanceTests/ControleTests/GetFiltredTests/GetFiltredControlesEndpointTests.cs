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
using System.Text.Json;
using wfc.referential.Application.Controles.Queries.GetFilteredControles;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ControleTests.GetFiltredTests;

public class GetFiltredControlesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IControleRepository> _repo = new();

    public GetFiltredControlesEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IControleRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Controle Make(string code, string? name = null, bool enabled = true)
    {
        var c = Controle.Create(ControleId.Of(Guid.NewGuid()), code, name ?? $"Name-{code}");
        if (!enabled) c.Disable();
        return c;
    }

    private record PagedDto<T>(T[] Items, int PageNumber, int PageSize,
                               int TotalCount, int TotalPages);


    [Fact(DisplayName = "GET /api/controles returns paged list")]
    public async Task Get_ShouldReturnPagedList()
    {
        var item1 = Make("C1", "Controle-1");
        var item2 = Make("C2", "Controle-2");

        var page = new PagedResult<Controle>(
            new List<Controle> { item1, item2 }, totalCount: 5, pageNumber: 1, pageSize: 2);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                        1, 2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var res = await _client.GetAsync("/api/controles?pageNumber=1&pageSize=2");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(5);
        dto.TotalPages.Should().Be(3);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        dto.Items[0].GetProperty("code").GetString().Should().Be("C1");
        dto.Items[0].GetProperty("name").GetString().Should().Be("Controle-1");
        dto.Items[1].GetProperty("code").GetString().Should().Be("C2");
        dto.Items[1].GetProperty("name").GetString().Should().Be("Controle-2");
    }

    [Fact(DisplayName = "GET /api/controles?code=ABC filters by Code")]
    public async Task Get_ShouldFilterByCode()
    {
        const string code = "ABC";
        var entity = Make(code);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q => q.Code == code),
                        1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new PagedResult<Controle>(new() { entity }, 1, 1, 10));

        var res = await _client.GetAsync($"/api/controles?code={code}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be(code);
    }

    [Fact(DisplayName = "GET /api/controles?name=Test filters by Name")]
    public async Task Get_ShouldFilterByName()
    {
        const string name = "Test";
        var entity = Make("T1", name);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q => q.Name == name),
                        1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new PagedResult<Controle>(new() { entity }, 1, 1, 10));

        var res = await _client.GetAsync($"/api/controles?name={name}");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("name").GetString().Should().Be(name);
    }

    [Fact(DisplayName = "GET /api/controles?isEnabled=false filters disabled")]
    public async Task Get_ShouldFilterByDisabled()
    {
        var disabled = Make("DIS", enabled: false);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q => q.IsEnabled == false),
                        1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new PagedResult<Controle>(new() { disabled }, 1, 1, 10));

        var res = await _client.GetAsync("/api/controles?isEnabled=false");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Single().GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/controles uses defaults when no params")]
    public async Task Get_ShouldUseDefaults()
    {
        var list = new[] { Make("A"), Make("B"), Make("C") };
        var page = new PagedResult<Controle>(list.ToList(), 3, 1, 10);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q => q.PageNumber == 1 && q.PageSize == 10 && q.IsEnabled == true),
                        1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var res = await _client.GetAsync("/api/controles");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(10);
        dto.Items.Should().HaveCount(3);

        dto.Items.Select(e => e.GetProperty("code").GetString())
                 .Should().BeEquivalentTo(new[] { "A", "B", "C" }, opts => opts.WithStrictOrdering());
    }

    [Fact(DisplayName = "GET /api/controles with multiple filters")]
    public async Task Get_ShouldApplyMultipleFilters()
    {
        var entity = Make("MULTI", "MultiName");
        var page = new PagedResult<Controle>(new() { entity }, 1, 1, 10);

        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q =>
                            q.Code == "MULTI" && q.Name == "MultiName" && q.IsEnabled == true),
                        1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(page);

        var res = await _client.GetAsync("/api/controles?code=MULTI&name=MultiName&isEnabled=true");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(1);
        dto.Items[0].GetProperty("code").GetString().Should().Be("MULTI");
    }

    [Fact(DisplayName = "GET /api/controles returns empty list when no match")]
    public async Task Get_ShouldReturnEmpty_WhenNoMatch()
    {
        _repo.Setup(r => r.GetPagedByCriteriaAsync(
                        It.Is<GetFilteredControlesQuery>(q => q.Code == "NONE"),
                        1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new PagedResult<Controle>(new(), 0, 1, 10));

        var res = await _client.GetAsync("/api/controles?code=NONE");
        var dto = await res.Content.ReadFromJsonAsync<PagedDto<JsonElement>>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().BeEmpty();
        dto.TotalCount.Should().Be(0);
        dto.TotalPages.Should().Be(0);
    }
}