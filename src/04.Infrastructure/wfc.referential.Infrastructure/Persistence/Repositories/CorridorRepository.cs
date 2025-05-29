using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class CorridorRepository :BaseRepository<Corridor,CorridorId>, ICorridorRepository
{
    public CorridorRepository(ApplicationDbContext context):base(context)
    {
    }

}