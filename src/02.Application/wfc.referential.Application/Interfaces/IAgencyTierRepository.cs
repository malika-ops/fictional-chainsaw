using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.AgencyTierAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IAgencyTierRepository : IRepositoryBase<AgencyTier, AgencyTierId>
{

}
