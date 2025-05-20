using wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.Interfaces
{
    public interface IParamTypeRepository
    {
        Task<List<ParamType>> GetAllParamTypesAsync(CancellationToken cancellationToken);
        IQueryable<ParamType> GetAllParamTypesQueryable(CancellationToken cancellationToken);
        Task<ParamType?> GetByIdAsync(ParamTypeId id, CancellationToken cancellationToken);
        Task<ParamType?> GetByValueAsync(string value, CancellationToken cancellationToken);
        Task<ParamType> AddParamTypeAsync(ParamType paramtype, CancellationToken cancellationToken);
        Task UpdateParamTypeAsync(ParamType paramtype, CancellationToken cancellationToken);
        Task DeleteParamTypeAsync(ParamType paramtype, CancellationToken cancellationToken);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<List<ParamType>> GetFilteredParamTypesAsync(GetAllParamTypesQuery request, CancellationToken cancellationToken);
        Task<int> GetCountTotalAsync(GetAllParamTypesQuery request, CancellationToken cancellationToken);
    }
}
