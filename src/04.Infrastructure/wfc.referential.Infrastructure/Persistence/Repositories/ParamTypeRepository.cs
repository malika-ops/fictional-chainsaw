using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ParamTypeRepository : BaseRepository<ParamType, ParamTypeId>, IParamTypeRepository
{
    public ParamTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

}