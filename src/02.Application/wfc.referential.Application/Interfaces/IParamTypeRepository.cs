using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.Interfaces
{
    public interface IParamTypeRepository : IRepositoryBase<ParamType,ParamTypeId>
    {
    }
}
