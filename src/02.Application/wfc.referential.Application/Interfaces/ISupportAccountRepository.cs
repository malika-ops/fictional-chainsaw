using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ISupportAccountRepository : IRepositoryBase<SupportAccount, SupportAccountId>
{
}