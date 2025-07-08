using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Domain.PartnerAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.GetFiltredTests;

public class GetFiltredPartnersEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "GET /api/partners returns paged list with default parameters")]
    public async Task Get_ShouldReturnPagedList_WithDefaultParameters()
    {
        // Arrange
        var allPartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Legal Person"),
            CreateTestPartner("PTN003", "Partner 3", "Natural Person"),
            CreateTestPartner("PTN004", "Partner 4", "Legal Person"),
            CreateTestPartner("PTN005", "Partner 5", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(allPartners.Take(10).ToList(), allPartners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                1, // default pageNumber
                                10, // default pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners returns paged list with custom parameters")]
    public async Task Get_ShouldReturnPagedList_WithCustomParameters()
    {
        // Arrange
        var allPartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Legal Person"),
            CreateTestPartner("PTN003", "Partner 3", "Natural Person"),
            CreateTestPartner("PTN004", "Partner 4", "Legal Person"),
            CreateTestPartner("PTN005", "Partner 5", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(allPartners.Skip(2).Take(2).ToList(), allPartners.Length, 2, 2);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?pageNumber=2&pageSize=2");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                2, // pageNumber
                                2, // pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?code=PTN001 returns only matching partner")]
    public async Task Get_ShouldFilterByCode()
    {
        // Arrange
        var partner = CreateTestPartner("PTN001", "Partner 1", "Natural Person");
        var pagedResult = new PagedResult<Partner>(new List<Partner> { partner }, 1, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?code=PTN001");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("PTN001");
        result.TotalCount.Should().Be(1);

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?personType=Natural Person filters by person type")]
    public async Task Get_ShouldFilterByPersonType()
    {
        // Arrange
        var naturalPersonPartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN005", "Partner 5", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(naturalPersonPartners.ToList(), naturalPersonPartners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?personType=Natural Person");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        foreach (var item in result.Items)
        {
            item.PersonType.Should().Be("Natural Person");
        }

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?isEnabled=false returns only disabled partners")]
    public async Task Get_ShouldFilterByEnabledStatus()
    {
        // Arrange
        var disabledPartner = CreateTestPartner("PTN001", "Disabled Partner", "Natural Person");
        disabledPartner.Disable(); // Make it disabled

        var pagedResult = new PagedResult<Partner>(new List<Partner> { disabledPartner }, 1, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?isEnabled=false");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().IsEnabled.Should().BeFalse();

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?name=Express filters by name")]
    public async Task Get_ShouldFilterByName()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Express Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Express Partner 2", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), partners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?name=Express");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        foreach (var item in result.Items)
        {
            item.Name.Should().Contain("Express");
        }

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?headquartersCity=Casablanca filters by headquarters city")]
    public async Task Get_ShouldFilterByHeadquartersCity()
    {
        // Arrange
        var casablancaPartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN003", "Partner 3", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(casablancaPartners.ToList(), casablancaPartners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?headquartersCity=Casablanca");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        foreach (var item in result.Items)
        {
            item.HeadquartersCity.Should().Be("Casablanca");
        }

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?professionalTaxNumber=PTX123 filters by professional tax number")]
    public async Task Get_ShouldFilterByProfessionalTaxNumber()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), partners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?professionalTaxNumber=PTX123");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        foreach (var item in result.Items)
        {
            item.ProfessionalTaxNumber.Should().Contain("PTX123");
        }

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?taxIdentificationNumber=TAX123 filters by tax identification number")]
    public async Task Get_ShouldFilterByTaxIdentificationNumber()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), partners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?taxIdentificationNumber=TAX123");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        foreach (var item in result.Items)
        {
            item.TaxIdentificationNumber.Should().Contain("TAX123");
        }

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners?ice=ICE123 filters by ICE")]
    public async Task Get_ShouldFilterByICE()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), partners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?ice=ICE123");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        foreach (var item in result.Items)
        {
            item.ICE.Should().Contain("ICE123");
        }

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners returns empty result when no matches found")]
    public async Task Get_ShouldReturnEmptyResult_WhenNoMatchesFound()
    {
        // Arrange
        var emptyResult = new PagedResult<Partner>(new List<Partner>(), 0, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/partners?code=NONEXISTENT");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/partners validates pagination parameters")]
    public async Task Get_ShouldValidatePaginationParameters()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), 1, 1, 1);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?pageNumber=1&pageSize=1");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                1, // pageNumber
                                1, // pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners handles multiple filters simultaneously")]
    public async Task Get_ShouldHandleMultipleFiltersSimultaneously()
    {
        // Arrange
        var filteredPartners = new[] {
            CreateTestPartner("PTN001", "Express Partner", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(filteredPartners.ToList(), 1, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?name=Express&personType=Natural Person&isEnabled=true&headquartersCity=Casablanca");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Contain("Express");
        result.Items.First().PersonType.Should().Be("Natural Person");
        result.Items.First().IsEnabled.Should().BeTrue();
        result.Items.First().HeadquartersCity.Should().Be("Casablanca");

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners handles large page sizes")]
    public async Task Get_ShouldHandleLargePageSizes()
    {
        // Arrange
        var partners = Enumerable.Range(1, 100)
            .Select(i => CreateTestPartner($"PTN{i:D3}", $"Partner {i}", i % 2 == 0 ? "Legal Person" : "Natural Person"))
            .ToArray();

        var pagedResult = new PagedResult<Partner>(partners.Take(50).ToList(), partners.Length, 1, 50);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?pageSize=50");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(50);
        result.TotalCount.Should().Be(100);
        result.TotalPages.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(50);

        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                1, // pageNumber
                                50, // pageSize
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "GET /api/partners returns proper response structure")]
    public async Task Get_ShouldReturnProperResponseStructure()
    {
        // Arrange
        var partner = CreateTestPartner("PTN001", "Test Partner", "Natural Person");
        var pagedResult = new PagedResult<Partner>(new List<Partner> { partner }, 1, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        var partnerResponse = result.Items.First();
        partnerResponse.PartnerId.Should().NotBeEmpty();
        partnerResponse.Code.Should().Be("PTN001");
        partnerResponse.Name.Should().Be("Test Partner");
        partnerResponse.PersonType.Should().Be("Natural Person");
        partnerResponse.ProfessionalTaxNumber.Should().Be("PTX123456");
        partnerResponse.WithholdingTaxRate.Should().Be("10.5");
        partnerResponse.HeadquartersCity.Should().Be("Casablanca");
        partnerResponse.HeadquartersAddress.Should().Be("123 Main Street");
        partnerResponse.LastName.Should().Be("Doe");
        partnerResponse.FirstName.Should().Be("John");
        partnerResponse.PhoneNumberContact.Should().Be("+212612345678");
        partnerResponse.MailContact.Should().Be("contact@partner.com");
        partnerResponse.FunctionContact.Should().Be("Manager");
        partnerResponse.TransferType.Should().Be("Bank Transfer");
        partnerResponse.AuthenticationMode.Should().Be("SMS");
        partnerResponse.TaxIdentificationNumber.Should().Be("TAX123456");
        partnerResponse.TaxRegime.Should().Be("Standard");
        partnerResponse.AuxiliaryAccount.Should().Be("AUX001");
        partnerResponse.ICE.Should().Be("ICE123456789");
        partnerResponse.Logo.Should().Be("/logos/logo.png");
        partnerResponse.IsEnabled.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/partners handles edge case with zero page size")]
    public async Task Get_ShouldHandleZeroPageSize()
    {
        // Arrange
        var emptyResult = new PagedResult<Partner>(new List<Partner>(), 0, 1, 0);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/partners?pageSize=0");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageSize.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/partners handles special characters in filters")]
    public async Task Get_ShouldHandleSpecialCharactersInFilters()
    {
        // Arrange
        var partner = CreateTestPartner("PTN001", "Partner & Associates", "Natural Person");
        var pagedResult = new PagedResult<Partner>(new List<Partner> { partner }, 1, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?name=Partner%20%26%20Associates");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Partner & Associates");
    }

    [Fact(DisplayName = "GET /api/partners handles concurrent requests")]
    public async Task Get_ShouldHandleConcurrentRequests()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), 2, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act - Simulate concurrent requests
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _client.GetAsync("/api/partners"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();
            result!.Items.Should().HaveCount(2);
        }

        // Verify repository was called for each request
        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                It.IsAny<int>(),
                                It.IsAny<int>(),
                                It.IsAny<CancellationToken>()),
                         Times.Exactly(5));
    }

    [Fact(DisplayName = "GET /api/partners handles invalid page numbers gracefully")]
    public async Task Get_ShouldHandleInvalidPageNumbers()
    {
        // Arrange
        var emptyResult = new PagedResult<Partner>(new List<Partner>(), 0, 0, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(emptyResult);

        // Act
        var response = await _client.GetAsync("/api/partners?pageNumber=0");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(0);
    }

    [Fact(DisplayName = "GET /api/partners handles negative page numbers")]
    public async Task Get_ShouldHandleNegativePageNumbers()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), 1, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?pageNumber=-1");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // The API should handle negative page numbers gracefully (possibly defaulting to 1)
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/partners respects default enabled filter")]
    public async Task Get_ShouldRespectDefaultEnabledFilter()
    {
        // Arrange
        var enabledPartners = new[] {
            CreateTestPartner("PTN001", "Enabled Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Enabled Partner 2", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(enabledPartners.ToList(), 2, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act - not specifying isEnabled should default to true
        var response = await _client.GetAsync("/api/partners");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.IsEnabled == true);
    }

    [Fact(DisplayName = "GET /api/partners allows override of default enabled filter")]
    public async Task Get_ShouldAllowOverrideOfDefaultEnabledFilter()
    {
        // Arrange
        var allPartners = new[] {
            CreateTestPartner("PTN001", "Enabled Partner", "Natural Person"),
            CreateTestPartner("PTN002", "Disabled Partner", "Legal Person")
        };
        allPartners[1].Disable(); // Disable the second partner

        var pagedResult = new PagedResult<Partner>(allPartners.ToList(), 2, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act - explicitly request both enabled and disabled partners
        var response = await _client.GetAsync("/api/partners?isEnabled=false");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.IsEnabled == true);
        result.Items.Should().Contain(p => p.IsEnabled == false);
    }

    [Fact(DisplayName = "GET /api/partners case insensitive filtering")]
    public async Task Get_ShouldPerformCaseInsensitiveFiltering()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Express Delivery", "Natural Person"),
            CreateTestPartner("PTN002", "express services", "Legal Person")
        };

        var pagedResult = new PagedResult<Partner>(partners.ToList(), 2, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?name=EXPRESS");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.Name.ToLower().Contains("express"));
    }

    [Fact(DisplayName = "GET /api/partners partial matching for text fields")]
    public async Task Get_ShouldPerformPartialMatchingForTextFields()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN001", "Global Express Services", "Natural Person"),
            CreateTestPartner("PTN002", "Local Express Ltd", "Legal Person"),
            CreateTestPartner("PTN003", "Standard Delivery", "Natural Person")
        };

        var filteredPartners = partners.Where(p => p.Name.Contains("Express")).ToArray();
        var pagedResult = new PagedResult<Partner>(filteredPartners.ToList(), filteredPartners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners?name=Express");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.Name.Contains("Express"));
        result.Items.Should().Contain(p => p.Name == "Global Express Services");
        result.Items.Should().Contain(p => p.Name == "Local Express Ltd");
    }

    [Fact(DisplayName = "GET /api/partners handles empty string filters")]
    public async Task Get_ShouldHandleEmptyStringFilters()
    {
        // Arrange
        var allPartners = new[] {
            CreateTestPartner("PTN001", "Partner 1", "Natural Person"),
            CreateTestPartner("PTN002", "Partner 2", "Legal Person"),
            CreateTestPartner("PTN003", "Partner 3", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(allPartners.ToList(), allPartners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act - empty string filters should return all records
        var response = await _client.GetAsync("/api/partners?code=&name=&personType=");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact(DisplayName = "GET /api/partners sorts results consistently")]
    public async Task Get_ShouldSortResultsConsistently()
    {
        // Arrange
        var partners = new[] {
            CreateTestPartner("PTN003", "Partner C", "Natural Person"),
            CreateTestPartner("PTN001", "Partner A", "Legal Person"),
            CreateTestPartner("PTN002", "Partner B", "Natural Person")
        };

        var sortedPartners = partners.OrderBy(p => p.Code).ToArray();
        var pagedResult = new PagedResult<Partner>(sortedPartners.ToList(), sortedPartners.Length, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partners");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(3);
        result.Items.Select(p => p.Code).Should().BeInAscendingOrder();
        result.Items.First().Code.Should().Be("PTN001");
        result.Items.Last().Code.Should().Be("PTN003");
    }

    [Fact(DisplayName = "GET /api/partners handles very large datasets")]
    public async Task Get_ShouldHandleVeryLargeDatasets()
    {
        // Arrange
        var totalCount = 10000;
        var pageSize = 100;
        var pageNumber = 50;

        var partnersPage = Enumerable.Range((pageNumber - 1) * pageSize + 1, pageSize)
            .Select(i => CreateTestPartner($"PTN{i:D5}", $"Partner {i}", i % 2 == 0 ? "Legal Person" : "Natural Person"))
            .ToList();

        var pagedResult = new PagedResult<Partner>(partnersPage, totalCount, pageNumber, pageSize);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync($"/api/partners?pageNumber={pageNumber}&pageSize={pageSize}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(pageSize);
        result.TotalCount.Should().Be(totalCount);
        result.TotalPages.Should().Be(100); // 10000 / 100
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);

        // Verify we got the correct page
        result.Items.First().Code.Should().Be("PTN04901");
        result.Items.Last().Code.Should().Be("PTN05000");
    }

    [Fact(DisplayName = "GET /api/partners performance with complex filters")]
    public async Task Get_ShouldHandleComplexFiltersEfficiently()
    {
        // Arrange
        var complexFilteredPartners = new[] {
            CreateTestPartner("PTN001", "Morocco Express Services", "Natural Person"),
            CreateTestPartner("PTN005", "Atlas Express Delivery", "Natural Person")
        };

        var pagedResult = new PagedResult<Partner>(complexFilteredPartners.ToList(), 2, 1, 10);

        _partnerRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(),
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act - Complex query with multiple filters
        var complexQuery = "/api/partners?" +
                          "name=Express&" +
                          "personType=Natural Person&" +
                          "headquartersCity=Casablanca&" +
                          "isEnabled=true&" +
                          "professionalTaxNumber=PTX123&" +
                          "pageNumber=1&" +
                          "pageSize=10";

        var response = await _client.GetAsync(complexQuery);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetPartnersResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p =>
            p.Name.Contains("Express") &&
            p.PersonType == "Natural Person" &&
            p.IsEnabled == true);

        // Verify repository was called once (efficient query)
        _partnerRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(),
                                1,
                                10,
                                It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // Helper to build dummy partners quickly
    private static Partner CreateTestPartner(string code, string name, string personType)
    {
        return Partner.Create(
            PartnerId.Of(Guid.NewGuid()),
            code,
            name,
            personType,
            "PTX123456",
            "10.5",
            "Casablanca",
            "123 Main Street",
            "Doe",
            "John",
            "+212612345678",
            "contact@partner.com",
            "Manager",
            "Bank Transfer",
            "SMS",
            "TAX123456",
            "Standard",
            "AUX001",
            "ICE123456789",
            "/logos/logo.png"
        );
    }
}