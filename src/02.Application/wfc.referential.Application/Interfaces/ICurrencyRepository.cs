using wfc.referential.Application.Currencies.Queries;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken);
    IQueryable<Currency> GetAllCurrenciesQueryable(CancellationToken cancellationToken);
    Task<Currency?> GetByIdAsync(CurrencyId id, CancellationToken cancellationToken);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<Currency?> GetByCodeIsoAsync(int codeiso, CancellationToken cancellationToken);
    Task<Currency> AddCurrencyAsync(Currency currency, CancellationToken cancellationToken);
    Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken);
    Task DeleteCurrencyAsync(Currency currency, CancellationToken cancellationToken);
    Task<bool> IsCurrencyAssociatedWithCountryAsync(CurrencyId id, CancellationToken cancellationToken);
    Task<List<Currency>> GetCurrenciesByCriteriaAsync(GetAllCurrenciesQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllCurrenciesQuery request, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}