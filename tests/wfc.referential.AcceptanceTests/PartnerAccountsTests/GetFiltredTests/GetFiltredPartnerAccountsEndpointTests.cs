using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using FluentAssertions;
using Moq;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerAccountsTests.GetFiltredTests;

public class GetFiltredPartnerAccountsEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    private static PartnerAccount CreateTestPartnerAccount(string accountNumber, string rib, string businessName, string accountTypeName)
    {
        var bankId = Guid.NewGuid();
        var bank = Bank.Create(BankId.Of(bankId), "AWB", "Attijariwafa Bank", "AWB");

        return PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            accountNumber,
            rib,
            "Casablanca Centre",
            businessName,
            businessName.Substring(0, 2).ToUpper(),
            50000.00m,
            bank,
            PartnerAccountTypeEnum.Activité
        );
    }

    private record PagedResultDto<T>(T[] Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

    [Fact(DisplayName = "GET /api/partner-accounts returns paged list")]
    public async Task Get_ShouldReturnPagedList_WhenParamsAreValid()
    {
        // Arrange
        var allAccounts = new[] {
            CreateTestPartnerAccount("000123456789", "12345678901234567890123", "Wafa Cash Services", "Activity"),
            CreateTestPartnerAccount("000987654321", "98765432109876543210987", "Transfert Express", "Commission")
        };

        var pagedResult = new PagedResult<PartnerAccount>(allAccounts.ToList(), 2, 1, 2);

        _partnerAccountRepoMock.Setup(r => r.GetPagedByCriteriaAsync(
                            It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(pagedResult);

        // Act
        var response = await _client.GetAsync("/api/partner-accounts?pageNumber=1&pageSize=2");
        var dto = await response.Content.ReadFromJsonAsync<PagedResult<PartnerAccountResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.Items.Should().HaveCount(2);
        dto.TotalCount.Should().Be(2);
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(2);

        _partnerAccountRepoMock.Verify(r => r.GetPagedByCriteriaAsync(
                                It.IsAny<object>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}
