using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.PartnerAccounts.Queries.GetFiltredPartnerAccounts;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPartnerAccountRepository : IRepositoryBase<PartnerAccount, PartnerAccountId>
{
    
}