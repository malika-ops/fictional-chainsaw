using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxeRuleDetails;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITaxRuleDetailRepository : IRepositoryBase<TaxRuleDetail, TaxRuleDetailsId>
{
    Task<List<TaxRuleDetail>> GetAllTaxRuleDetailsAsync(CancellationToken cancellationToken);
    Task<TaxRuleDetail?> GetTaxRuleDetailByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TaxRuleDetail> AddTaxRuleDetailAsync(TaxRuleDetail tax, CancellationToken cancellationToken);
    Task UpdateTaxRuleDetailAsync(TaxRuleDetail tax, CancellationToken cancellationToken);
    Task DeleteTaxRuleDetailAsync(TaxRuleDetail tax, CancellationToken cancellationToken);

    Task<List<TaxRuleDetail>> GetTaxRuleDetailsByCriteriaAsync(GetAllTaxRuleDetailsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllTaxRuleDetailsQuery request, CancellationToken cancellationToken);
    Task<TaxRuleDetail?> GetByCorridorTaxServiceAsync(CorridorId corridorId, TaxId taxId, ServiceId serviceId, CancellationToken cancellationToken);
}
