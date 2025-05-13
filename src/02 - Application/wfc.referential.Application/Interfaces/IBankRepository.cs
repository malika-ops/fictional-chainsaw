using wfc.referential.Application.Banks.Queries.GetAllBanks;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IBankRepository
{
    Task<List<Bank>> GetAllBanksAsync(CancellationToken cancellationToken);
    IQueryable<Bank> GetAllBanksQueryable(CancellationToken cancellationToken);
    Task<Bank?> GetByIdAsync(BankId id, CancellationToken cancellationToken);
    Task<Bank?> GetByCodeAsync(string bankCode, CancellationToken cancellationToken);
    Task<Bank> AddBankAsync(Bank bank, CancellationToken cancellationToken);
    Task UpdateBankAsync(Bank bank, CancellationToken cancellationToken);
    Task DeleteBankAsync(Bank bank, CancellationToken cancellationToken);
    Task<List<Bank>> GetFilteredBanksAsync(GetAllBanksQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllBanksQuery request, CancellationToken cancellationToken);
    Task<bool> HasLinkedAccountsAsync(BankId bankId, CancellationToken cancellationToken);
}