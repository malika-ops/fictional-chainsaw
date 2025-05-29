using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPartnerRepository : IRepositoryBase<Partner, PartnerId>
{
}