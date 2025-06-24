using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class TaxRuleDetailRepository : BaseRepository<TaxRuleDetail, TaxRuleDetailsId>, ITaxRuleDetailRepository
{
    public TaxRuleDetailRepository(ApplicationDbContext context) : base(context)
    {
    }
}