using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IAffiliateRepository : IRepositoryBase<Affiliate, AffiliateId>
{
}