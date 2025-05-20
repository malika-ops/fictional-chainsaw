using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ISupportAccountRepository
{
    Task<List<SupportAccount>> GetAllSupportAccountsAsync(CancellationToken cancellationToken);

    IQueryable<SupportAccount> GetAllSupportAccountsQueryable(CancellationToken cancellationToken);

    Task<SupportAccount?> GetByIdAsync(SupportAccountId id, CancellationToken cancellationToken);

    Task<SupportAccount?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<SupportAccount?> GetByAccountingNumberAsync(string accountingNumber, CancellationToken cancellationToken);

    Task<SupportAccount> AddSupportAccountAsync(SupportAccount supportAccount, CancellationToken cancellationToken);

    Task UpdateSupportAccountAsync(SupportAccount supportAccount, CancellationToken cancellationToken);

    Task DeleteSupportAccountAsync(SupportAccount supportAccount, CancellationToken cancellationToken);

    Task<List<SupportAccount>> GetFilteredSupportAccountsAsync(GetAllSupportAccountsQuery request, CancellationToken cancellationToken);

    Task<int> GetCountTotalAsync(GetAllSupportAccountsQuery request, CancellationToken cancellationToken);

    Task<List<SupportAccount>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken);
}