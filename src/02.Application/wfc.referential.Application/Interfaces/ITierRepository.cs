using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITierRepository : IRepositoryBase<Tier, TierId>
{
 }
