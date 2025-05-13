using wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPartnerAccountRepository
{
    Task<List<PartnerAccount>> GetAllPartnerAccountsAsync(CancellationToken cancellationToken);
    IQueryable<PartnerAccount> GetAllPartnerAccountsQueryable(CancellationToken cancellationToken);
    Task<PartnerAccount?> GetByIdAsync(PartnerAccountId id, CancellationToken cancellationToken);
    Task<PartnerAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<PartnerAccount?> GetByRIBAsync(string rib, CancellationToken cancellationToken);
    Task<PartnerAccount> AddPartnerAccountAsync(PartnerAccount partnerAccount, CancellationToken cancellationToken);
    Task UpdatePartnerAccountAsync(PartnerAccount partnerAccount, CancellationToken cancellationToken);
    Task DeletePartnerAccountAsync(PartnerAccount partnerAccount, CancellationToken cancellationToken);
    Task<List<PartnerAccount>> GetFilteredPartnerAccountsAsync(GetAllPartnerAccountsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllPartnerAccountsQuery request, CancellationToken cancellationToken);
    Task<List<PartnerAccount>> GetByBankIdAsync(Guid bankId, CancellationToken cancellationToken);
}