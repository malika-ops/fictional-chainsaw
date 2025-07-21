using System.Net;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Operators.Dtos;
using wfc.referential.Domain.OperatorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.OperatorsTests.GetFiltredTests;

public class GetFiltredOperatorsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "GET /api/operators returns 200 with default pagination")]
    public async Task Get_ShouldReturn200_WithDefaultPagination()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(5);
        var pagedResult = new PagedResult<Operator>(mockOperators, 5, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        // Verify repository was called with default parameters
        _operatorRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            1, // Default page number
            10, // Default page size
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with custom pagination")]
    public async Task Get_ShouldReturn200_WithCustomPagination()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(3);
        var pagedResult = new PagedResult<Operator>(mockOperators, 15, 2, 5);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?PageNumber=2&PageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);

        // Verify repository was called with custom parameters
        _operatorRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            2, // Custom page number
            5, // Custom page size
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with code filter")]
    public async Task Get_ShouldReturn200_WithCodeFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(1, "OP001");
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?Code=OP001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("OP001");

        _operatorRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with identity code filter")]
    public async Task Get_ShouldReturn200_WithIdentityCodeFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(1, identityCode: "ID123456");
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?IdentityCode=ID123456");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().IdentityCode.Should().Be("ID123456");
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with last name filter")]
    public async Task Get_ShouldReturn200_WithLastNameFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(1, lastName: "Alami");
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?LastName=Alami");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().LastName.Should().Be("Alami");
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with first name filter")]
    public async Task Get_ShouldReturn200_WithFirstNameFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(1, firstName: "Ahmed");
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?FirstName=Ahmed");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().FirstName.Should().Be("Ahmed");
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with email filter")]
    public async Task Get_ShouldReturn200_WithEmailFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(1, email: "ahmed.alami@wafacash.com");
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?Email=ahmed.alami@wafacash.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Email.Should().Be("ahmed.alami@wafacash.com");
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with OperatorType filter")]
    public async Task Get_ShouldReturn200_WithOperatorTypeFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(2, operatorType: OperatorType.Agence);
        var pagedResult = new PagedResult<Operator>(mockOperators, 2, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/operators?OperatorType={(int)OperatorType.Agence}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(o => o.OperatorType == OperatorType.Agence);
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with BranchId filter")]
    public async Task Get_ShouldReturn200_WithBranchIdFilter()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var mockOperators = CreateMockOperatorsList(2, branchId: branchId);
        var pagedResult = new PagedResult<Operator>(mockOperators, 2, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/operators?BranchId={branchId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(o => o.BranchId == branchId);
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with IsEnabled filter")]
    public async Task Get_ShouldReturn200_WithIsEnabledFilter()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(3, isEnabled: false);
        var pagedResult = new PagedResult<Operator>(mockOperators, 3, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?IsEnabled=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Items.Should().OnlyContain(o => o.IsEnabled == false);
    }

    [Fact(DisplayName = "GET /api/operators returns 200 with multiple filters")]
    public async Task Get_ShouldReturn200_WithMultipleFilters()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var mockOperators = CreateMockOperatorsList(1, "OP001", "ID123456", "Alami", "Ahmed", "ahmed.alami@wafacash.com", OperatorType.Agence, branchId, true);
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/operators?Code=OP001&LastName=Alami&FirstName=Ahmed&Email=ahmed.alami@wafacash.com&OperatorType={(int)OperatorType.Agence}&BranchId={branchId}&IsEnabled=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var operatorResponse = result.Items.First();
        operatorResponse.Code.Should().Be("OP001");
        operatorResponse.LastName.Should().Be("Alami");
        operatorResponse.FirstName.Should().Be("Ahmed");
        operatorResponse.Email.Should().Be("ahmed.alami@wafacash.com");
        operatorResponse.OperatorType.Should().Be(OperatorType.Agence);
        operatorResponse.BranchId.Should().Be(branchId);
        operatorResponse.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/operators returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var pagedResult = new PagedResult<Operator>(new List<Operator>(), 0, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?Code=NONEXISTENT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "GET /api/operators handles large page sizes")]
    public async Task Get_ShouldHandleLargePageSizes()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(100);
        var pagedResult = new PagedResult<Operator>(mockOperators, 1000, 1, 100);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/operators?PageSize=100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(100);
        result.PageSize.Should().Be(100);
    }

    [Fact(DisplayName = "GET /api/operators handles repository exceptions gracefully")]
    public async Task Get_ShouldHandleRepositoryExceptionsGracefully()
    {
        // Arrange
        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await _client.GetAsync("/api/operators");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "GET /api/operators supports case-insensitive filtering")]
    public async Task Get_ShouldSupportCaseInsensitiveFiltering()
    {
        // Arrange
        var mockOperators = CreateMockOperatorsList(1, "OP001", lastName: "Alami");
        var pagedResult = new PagedResult<Operator>(mockOperators, 1, 1, 10);

        _operatorRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act - Test with different case
        var response = await _client.GetAsync("/api/operators?LastName=alami");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetOperatorsResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    // Helper Methods
    private static List<Operator> CreateMockOperatorsList(
        int count,
        string code = null,
        string identityCode = null,
        string lastName = null,
        string firstName = null,
        string email = null,
        OperatorType? operatorType = null,
        Guid? branchId = null,
        bool? isEnabled = null)
    {
        var operators = new List<Operator>();

        for (int i = 0; i < count; i++)
        {
            var operatorEntity = Operator.Create(
                OperatorId.Of(Guid.NewGuid()),
                code ?? $"OP{i:D3}",
                identityCode ?? $"ID{i:D6}",
                lastName ?? $"LastName{i}",
                firstName ?? $"FirstName{i}",
                email ?? $"operator{i}@email.com",
                "+212600000000",
                operatorType ?? OperatorType.Agence,
                branchId ?? Guid.NewGuid());

            if (isEnabled.HasValue && !isEnabled.Value)
            {
                operatorEntity.Disable();
            }

            operators.Add(operatorEntity);
        }

        return operators;
    }
}