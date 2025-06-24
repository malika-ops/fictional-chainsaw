using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Contracts.Dtos;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.PartnerAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractsTests.GetFiltredTests;

public class GetFiltredContractsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IContractRepository> _repoMock = new();

    public GetFiltredContractsEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IContractRepository>();
                services.RemoveAll<ICacheService>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "GET /api/contracts returns paged list with default parameters")]
    public async Task Get_ShouldReturnPagedList_WithDefaultParameters()
    {
        // Arrange
        var allContracts = new[] {
            CreateTestContract("CTR001", Guid.NewGuid()),
            CreateTestContract("CTR002", Guid.NewGuid()),
            CreateTestContract("CTR003", Guid.NewGuid()),
            CreateTestContract("CTR004", Guid.NewGuid()),
            CreateTestContract("CTR005", Guid.NewGuid())
        };

        var pagedResult = new PagedResult<Contract>(allContracts.Take(10).ToList(), allContracts.Length, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contracts");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        _repoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                1, // default pageNumber
                                10, // default pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/contracts?code=CTR001 returns only matching contract")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var contract = CreateTestContract("CTR001", Guid.NewGuid());
        var pagedResult = new PagedResult<Contract>(new List<Contract> { contract }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contracts?code=CTR001");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("CTR001");
        result.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "GET /api/contracts?partnerId=guid filters by partner ID")]
    public async Task Get_ShouldFilterByPartnerId()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var contracts = new[] {
            CreateTestContract("CTR001", partnerId),
            CreateTestContract("CTR002", partnerId)
        };

        var pagedResult = new PagedResult<Contract>(contracts.ToList(), contracts.Length, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/contracts?partnerId={partnerId}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        foreach (var item in result.Items)
        {
            item.PartnerId.Should().Be(partnerId);
        }
    }

    [Fact(DisplayName = "GET /api/contracts?isEnabled=false returns only disabled contracts")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledContract = CreateTestContract("CTR001", Guid.NewGuid());
        disabledContract.Disable(); // Make it disabled

        var pagedResult = new PagedResult<Contract>(new List<Contract> { disabledContract }, 1, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contracts?isEnabled=false");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/contracts returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<Contract>(new List<Contract>(), 0, 1, 10);

        _repoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/contracts?code=NONEXISTENT");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // Helper to build dummy contracts quickly
    private static Contract CreateTestContract(string code, Guid partnerId)
    {
        return Contract.Create(
            ContractId.Of(Guid.NewGuid()),
            code,
            PartnerId.Of(partnerId),
            DateTime.Today,
            DateTime.Today.AddDays(365));
    }
}