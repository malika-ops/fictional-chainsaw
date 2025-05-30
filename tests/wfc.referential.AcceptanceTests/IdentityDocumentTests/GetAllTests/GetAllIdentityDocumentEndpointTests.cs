using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;
using wfc.referential.Domain.IdentityDocumentAggregate;
using BuildingBlocks.Core.Pagination;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.GetAllTests;

public class GetAllIdentityDocumentsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IIdentityDocumentRepository> _repoMock = new();

    public GetAllIdentityDocumentsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IIdentityDocumentRepository>();

                services.AddSingleton(_repoMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static IdentityDocument Dummy(string code, string name) =>
        IdentityDocument.Create(IdentityDocumentId.Of(Guid.NewGuid()), code, name, null);

    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/identitydocuments returns paginated result")]
    public async Task Get_ShouldReturnPaginated_WhenPagingIsApplied()
    {
        var data = new[]
        {
            Dummy("CIN", "Carte Nationale"),
            Dummy("PASSPORT", "Passeport"),
            Dummy("NID", "ID Étrangère")
        };

        var pagedResult = new PagedResult<IdentityDocument>(
            data.Take(2).ToList(),
            totalCount: 3,
            pageNumber: 1,
            pageSize: 2
        );

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.Is<GetAllIdentityDocumentsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                    1, 2,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        var response = await _client.GetAsync("/api/identitydocuments?pageNumber=1&pageSize=2");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }

    [Fact(DisplayName = "GET /api/identitydocuments?code=CIN filters correctly")]
    public async Task Get_ShouldFilterByCode()
    {
        var cin = Dummy("CIN", "Carte Nationale");

        var pagedResult = new PagedResult<IdentityDocument>(
            new List<IdentityDocument> { cin },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10
        );

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.Is<GetAllIdentityDocumentsQuery>(q => q.Code == "CIN"),
                    1, 10,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        var response = await _client.GetAsync("/api/identitydocuments?code=CIN");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items[0].GetProperty("code").GetString().Should().Be("CIN");
    }

    [Fact(DisplayName = "GET /api/identitydocuments defaults to page 1 size 10")]
    public async Task Get_ShouldUseDefaults_WhenNoParamsProvided()
    {
        var data = new[] { Dummy("A", "Alpha"), Dummy("B", "Beta"), Dummy("C", "Gamma") };

        var pagedResult = new PagedResult<IdentityDocument>(
            data.ToList(),
            totalCount: 3,
            pageNumber: 1,
            pageSize: 10
        );

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.Is<GetAllIdentityDocumentsQuery>(q => q.PageNumber == 1 && q.PageSize == 10),
                    1, 10,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        var response = await _client.GetAsync("/api/identitydocuments");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "GET /api/identitydocuments?name=Carte filters by name")]
    public async Task Get_ShouldFilterByName()
    {
        var carte = Dummy("CIN", "Carte Nationale");

        var pagedResult = new PagedResult<IdentityDocument>(
            new List<IdentityDocument> { carte },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10
        );

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.Is<GetAllIdentityDocumentsQuery>(q => q.Name == "Carte"),
                    1, 10,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        var response = await _client.GetAsync("/api/identitydocuments?name=Carte");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items[0].GetProperty("name").GetString().Should().Be("Carte Nationale");
    }

    [Fact(DisplayName = "GET /api/identitydocuments?isEnabled=false filters by status")]
    public async Task Get_ShouldFilterByStatus()
    {
        var disabledDoc = Dummy("DISABLED", "Disabled Document");
        disabledDoc.Disable();

        var pagedResult = new PagedResult<IdentityDocument>(
            new List<IdentityDocument> { disabledDoc },
            totalCount: 1,
            pageNumber: 1,
            pageSize: 10
        );

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                    It.Is<GetAllIdentityDocumentsQuery>(q => q.IsEnabled == false),
                    1, 10,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(pagedResult));

        var response = await _client.GetAsync("/api/identitydocuments?isEnabled=false");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<JsonElement>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items[0].GetProperty("isEnabled").GetBoolean().Should().BeFalse();
    }
}