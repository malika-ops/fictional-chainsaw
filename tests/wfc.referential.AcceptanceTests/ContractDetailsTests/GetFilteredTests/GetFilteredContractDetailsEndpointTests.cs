using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.ContractDetails.Dtos;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Domain.PricingAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ContractDetailsTests.GetFilteredTests;

public class GetFilteredContractDetailsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "GET /api/contractdetails returns paged list with default parameters")]
    public async Task Get_ShouldReturnPagedList_WithDefaultParameters()
    {
        // Arrange
        var allContractDetails = new[] {
            CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid()),
            CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid()),
            CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid()),
            CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid()),
            CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid())
        };

        var pagedResult = new PagedResult<ContractDetails>(allContractDetails.Take(10).ToList(), allContractDetails.Length, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contractdetails");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        _contractDetailsRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                1, // default pageNumber
                                10, // default pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/contractdetails?contractId=guid filters by contract ID")]
    public async Task Get_ShouldFilterByContractId()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var contractDetails = new[] {
            CreateTestContractDetails(contractId, Guid.NewGuid()),
            CreateTestContractDetails(contractId, Guid.NewGuid())
        };

        var pagedResult = new PagedResult<ContractDetails>(contractDetails.ToList(), contractDetails.Length, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/contractdetails?contractId={contractId}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        foreach (var item in result.Items)
        {
            item.ContractId.Should().Be(contractId);
        }
    }

    [Fact(DisplayName = "GET /api/contractdetails?pricingId=guid filters by pricing ID")]
    public async Task Get_ShouldFilterByPricingId()
    {
        // Arrange
        var pricingId = Guid.NewGuid();
        var contractDetails = new[] {
            CreateTestContractDetails(Guid.NewGuid(), pricingId),
            CreateTestContractDetails(Guid.NewGuid(), pricingId)
        };

        var pagedResult = new PagedResult<ContractDetails>(contractDetails.ToList(), contractDetails.Length, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/contractdetails?pricingId={pricingId}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        foreach (var item in result.Items)
        {
            item.PricingId.Should().Be(pricingId);
        }
    }

    [Fact(DisplayName = "GET /api/contractdetails?isEnabled=false returns only disabled contract details")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledContractDetails = CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid());
        disabledContractDetails.Disable(); // Make it disabled

        var pagedResult = new PagedResult<ContractDetails>(new List<ContractDetails> { disabledContractDetails }, 1, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contractdetails?isEnabled=false");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().IsEnabled.Should().BeFalse();
    }

    [Fact(DisplayName = "GET /api/contractdetails returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<ContractDetails>(new List<ContractDetails>(), 0, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/contractdetails?contractId=" + Guid.NewGuid());
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/contractdetails with multiple filters applies all filters")]
    public async Task Get_ShouldApplyMultipleFilters_WhenMultipleFiltersProvided()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(contractId, pricingId);

        var pagedResult = new PagedResult<ContractDetails>(new List<ContractDetails> { contractDetails }, 1, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/contractdetails?contractId={contractId}&pricingId={pricingId}&isEnabled=true");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().ContractId.Should().Be(contractId);
        result.Items.First().PricingId.Should().Be(pricingId);
        result.Items.First().IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/contractdetails supports pagination parameters")]
    public async Task Get_ShouldSupportPaginationParameters()
    {
        // Arrange
        var allContractDetails = Enumerable.Range(1, 25)
            .Select(_ => CreateTestContractDetails(Guid.NewGuid(), Guid.NewGuid()))
            .ToArray();

        // Return page 2 with 5 items per page
        var pagedResult = new PagedResult<ContractDetails>(
            allContractDetails.Skip(5).Take(5).ToList(),
            allContractDetails.Length,
            2,
            5);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contractdetails?pageNumber=2&pageSize=5");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(5);

        _contractDetailsRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                2, // requested pageNumber
                                5, // requested pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/contractdetails returns proper structure for each item")]
    public async Task Get_ShouldReturnProperStructure_ForEachItem()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        var contractDetails = CreateTestContractDetails(contractId, pricingId);

        var pagedResult = new PagedResult<ContractDetails>(new List<ContractDetails> { contractDetails }, 1, 1, 10);

        _contractDetailsRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/contractdetails");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetContractDetailsResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);

        var item = result.Items.First();
        item.ContractDetailsId.Should().Be(contractDetails.Id.Value);
        item.ContractId.Should().Be(contractId);
        item.PricingId.Should().Be(pricingId);
        item.IsEnabled.Should().BeTrue();
    }

    // Helper to build dummy contract details quickly
    private static ContractDetails CreateTestContractDetails(Guid contractId, Guid pricingId)
    {
        return ContractDetails.Create(
            ContractDetailsId.Of(Guid.NewGuid()),
            ContractId.Of(contractId),
            PricingId.Of(pricingId));
    }
}