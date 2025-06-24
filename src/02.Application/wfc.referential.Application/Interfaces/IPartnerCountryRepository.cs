using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPartnerCountryRepository :IRepositoryBase<PartnerCountry,PartnerCountryId>
{

}
