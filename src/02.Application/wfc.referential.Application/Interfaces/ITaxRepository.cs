using wfc.referential.Application.Taxes.Queries.GetAllTaxes;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITaxRepository
{
    Task<List<Tax>> GetAllTaxesAsync(CancellationToken cancellationToken);
    Task<Tax?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Tax?> GetByCodeAsync(string taxCode, CancellationToken cancellationToken);

    Task<Tax> AddTaxAsync(Tax tax, CancellationToken cancellationToken);
    Task UpdateTaxAsync(Tax tax, CancellationToken cancellationToken);
    Task DeleteTaxAsync(Tax tax, CancellationToken cancellationToken);

    Task<List<Tax>> GetTaxesByCriteriaAsync(GetAllTaxesQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllTaxesQuery request, CancellationToken cancellationToken);

    Task<bool> HasTaxRuleDetailsAsync(TaxId taxId, CancellationToken cancellationToken);
}
