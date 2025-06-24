using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ContractAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class ContractRepository : BaseRepository<Contract, ContractId>, IContractRepository
{
    public ContractRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}