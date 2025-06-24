using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITaxRuleDetailRepository : IRepositoryBase<TaxRuleDetail, TaxRuleDetailsId>
{

}
