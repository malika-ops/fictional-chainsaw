using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IAgencyRepository: IRepositoryBase<Agency,AgencyId>
{

}
