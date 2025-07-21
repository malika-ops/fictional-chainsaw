using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class OperatorRepository : BaseRepository<Operator, OperatorId>, IOperatorRepository
{
    public OperatorRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}