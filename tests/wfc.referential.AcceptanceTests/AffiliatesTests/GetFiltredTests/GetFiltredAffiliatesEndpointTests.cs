using System.Net;
using System.Text.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Affiliates.Dtos;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.AffiliatesTests.GetFiltredTests;

public class GetFiltredAffiliatesEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "GET /api/affiliates returns 200 with default pagination")]
    public async Task Get_ShouldReturn200_WithDefaultPagination()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(5);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 5, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        // Verify repository was called with default parameters
        _affiliateRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            1, // Default page number
            10, // Default page size
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with custom pagination")]
    public async Task Get_ShouldReturn200_WithCustomPagination()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(3);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 15, 2, 5);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates?PageNumber=2&PageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);

        // Verify repository was called with custom parameters
        _affiliateRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            2, // Custom page number
            5, // Custom page size
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with code filter")]
    public async Task Get_ShouldReturn200_WithCodeFilter()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(1, "AFF001");
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates?Code=AFF001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("AFF001");

        _affiliateRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with name filter")]
    public async Task Get_ShouldReturn200_WithNameFilter()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(1, name: "Wafacash");
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates?Name=Wafacash");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Wafacash");

        _affiliateRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with opening date filter")]
    public async Task Get_ShouldReturn200_WithOpeningDateFilter()
    {
        // Arrange
        var openingDate = new DateTime(2023, 1, 15);
        var mockAffiliates = CreateMockAffiliatesList(1, openingDate: openingDate);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/affiliates?OpeningDate={openingDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().OpeningDate.Should().Be(openingDate);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with cancellation day filter")]
    public async Task Get_ShouldReturn200_WithCancellationDayFilter()
    {
        // Arrange
        var cancellationDay = "15th of month";
        var mockAffiliates = CreateMockAffiliatesList(1, cancellationDay: cancellationDay);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/affiliates?CancellationDay={Uri.EscapeDataString(cancellationDay)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CancellationDay.Should().Be(cancellationDay);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with CountryId filter")]
    public async Task Get_ShouldReturn200_WithCountryIdFilter()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var mockAffiliates = CreateMockAffiliatesList(2, countryId: countryId);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 2, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/affiliates?CountryId={countryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(a => a.CountryId == countryId);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with IsEnabled filter")]
    public async Task Get_ShouldReturn200_WithIsEnabledFilter()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(3, isEnabled: false);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 3, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates?IsEnabled=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Items.Should().OnlyContain(a => a.IsEnabled == false);
    }

    [Fact(DisplayName = "GET /api/affiliates returns 200 with multiple filters")]
    public async Task Get_ShouldReturn200_WithMultipleFilters()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var openingDate = new DateTime(2023, 1, 15);
        var cancellationDay = "Last day of month";
        var mockAffiliates = CreateMockAffiliatesList(1, "AFF001", "Wafacash", openingDate, cancellationDay, countryId, true);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/affiliates?Code=AFF001&Name=Wafacash&OpeningDate={openingDate:yyyy-MM-dd}&CancellationDay={Uri.EscapeDataString(cancellationDay)}&CountryId={countryId}&IsEnabled=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var affiliate = result.Items.First();
        affiliate.Code.Should().Be("AFF001");
        affiliate.Name.Should().Be("Wafacash");
        affiliate.OpeningDate.Should().Be(openingDate);
        affiliate.CancellationDay.Should().Be(cancellationDay);
        affiliate.CountryId.Should().Be(countryId);
        affiliate.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/affiliates returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var pagedResult = new PagedResult<Affiliate>(new List<Affiliate>(), 0, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates?Code=NONEXISTENT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "GET /api/affiliates handles large page sizes")]
    public async Task Get_ShouldHandleLargePageSizes()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(100);
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1000, 1, 100);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/affiliates?PageSize=100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(100);
        result.PageSize.Should().Be(100);
    }

    [Fact(DisplayName = "GET /api/affiliates handles repository exceptions gracefully")]
    public async Task Get_ShouldHandleRepositoryExceptionsGracefully()
    {
        // Arrange
        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var response = await _client.GetAsync("/api/affiliates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "GET /api/affiliates supports case-insensitive filtering")]
    public async Task Get_ShouldSupportCaseInsensitiveFiltering()
    {
        // Arrange
        var mockAffiliates = CreateMockAffiliatesList(1, "AFF001", "Wafacash");
        var pagedResult = new PagedResult<Affiliate>(mockAffiliates, 1, 1, 10);

        _affiliateRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act - Test with different case
        var response = await _client.GetAsync("/api/affiliates?Name=wafacash");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<GetAffiliatesResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    // Helper Methods
    private static List<Affiliate> CreateMockAffiliatesList(
        int count,
        string code = null,
        string name = null,
        DateTime? openingDate = null,
        string cancellationDay = null,
        Guid? countryId = null,
        bool? isEnabled = null)
    {
        var affiliates = new List<Affiliate>();

        for (int i = 0; i < count; i++)
        {
            var affiliate = Affiliate.Create(
                AffiliateId.Of(Guid.NewGuid()),
                code ?? $"AFF{i:D3}",
                name ?? $"Affiliate {i}",
                "WFC", // Default abbreviation - not used in filtering anymore
                openingDate ?? DateTime.Now.AddDays(-30),
                cancellationDay ?? "Last day of month",
                "/logos/affiliate.png",
                10000.00m,
                "ACC-DOC-001",
                "411000001",
                "Stamp duty applicable",
                CountryId.Of(countryId ?? Guid.NewGuid()));

            if (isEnabled.HasValue && !isEnabled.Value)
            {
                // If your Affiliate entity has a Disable method, use it
                // Otherwise, we'll need to use reflection or another approach
                // affiliate.Disable();

                // Alternative: Use reflection if no Disable method exists
                var isEnabledProperty = typeof(Affiliate).GetProperty("IsEnabled");
                if (isEnabledProperty != null && isEnabledProperty.CanWrite)
                {
                    isEnabledProperty.SetValue(affiliate, false);
                }
            }

            affiliates.Add(affiliate);
        }

        return affiliates;
    }
}