using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class ControleRepository : BaseRepository<Controle, ControleId>, IControleRepository
{
    public ControleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}