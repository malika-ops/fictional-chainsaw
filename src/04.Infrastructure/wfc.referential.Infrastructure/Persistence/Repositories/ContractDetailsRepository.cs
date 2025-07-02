using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractDetailsAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class ContractDetailsRepository : BaseRepository<ContractDetails, ContractDetailsId>, IContractDetailsRepository
{
    public ContractDetailsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}