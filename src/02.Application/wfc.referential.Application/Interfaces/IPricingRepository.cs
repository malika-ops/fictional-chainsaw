using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.PricingAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPricingRepository : IRepositoryBase<Pricing, PricingId>
{
}