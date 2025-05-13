using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxeRuleDetails;
using wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxRuleDetails;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxRuleDetailTests;

public class GetAllTaxRuleDetailsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRuleDetailRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private const string BaseUrl = "api/taxruledetails";

    public GetAllTaxRuleDetailsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRuleDetailRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

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
        _repoMock.Setup(r => r.GetTaxRuleDetailsByCriteriaAsync(
                            It.Is<GetAllTaxRuleDetailsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allTaxRuleDetails.Take(2).ToList());

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTaxRuleDetailsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(allTaxRuleDetails.Length);

        // Cache service returns null to force repository call
        _cacheMock.Setup(c => c.GetAsync<PagedResult<GetAllTaxRuleDetailsResponse>>(
                                It.IsAny<string>(),
                                It.IsAny<CancellationToken>()))
                  .ReturnsAsync((PagedResult<GetAllTaxRuleDetailsResponse>)null);

        _cacheMock.Setup(c => c.SetAsync(
                                It.IsAny<string>(),
                                It.IsAny<object>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

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

        _repoMock.Verify(r => r.GetTaxRuleDetailsByCriteriaAsync(
                                It.Is<GetAllTaxRuleDetailsQuery>(q => q.PageNumber == 1 && q.PageSize == 2),
                                It.IsAny<CancellationToken>()),
                         Times.Once);

        _repoMock.Verify(r => r.GetCountTotalAsync(
                                It.IsAny<GetAllTaxRuleDetailsQuery>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);

        _cacheMock.Verify(c => c.SetAsync(
                                It.IsAny<string>(),
                                It.IsAny<object>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = $"GET {BaseUrl}?appliedOn=Fees returns filtered list")]
    public async Task Get_ShouldFilterByAppliedOn()
    {
        // Arrange
        var filteredTaxRuleDetail = CreateDummyTaxRuleDetail(Guid.NewGuid());

        _repoMock.Setup(r => r.GetTaxRuleDetailsByCriteriaAsync(
                            It.Is<GetAllTaxRuleDetailsQuery>(q => q.AppliedOn == ApplicationRule.Fees),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<TaxRuleDetail> { filteredTaxRuleDetail });

        _repoMock.Setup(r => r.GetCountTotalAsync(
                            It.IsAny<GetAllTaxRuleDetailsQuery>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(1);

        _cacheMock.Setup(c => c.GetAsync<PagedResult<GetAllTaxRuleDetailsResponse>>(
                                It.IsAny<string>(),
                                It.IsAny<CancellationToken>()))
                  .ReturnsAsync((PagedResult<GetAllTaxRuleDetailsResponse>)null);

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

        _repoMock.Verify(r => r.GetTaxRuleDetailsByCriteriaAsync(
                                It.Is<GetAllTaxRuleDetailsQuery>(q => q.AppliedOn == ApplicationRule.Fees),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}
