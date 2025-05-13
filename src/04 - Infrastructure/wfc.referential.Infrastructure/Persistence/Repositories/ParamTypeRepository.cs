using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ParamTypeRepository : IParamTypeRepository
{
    private readonly ApplicationDbContext _context;

    public ParamTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ParamType>> GetAllParamTypesAsync(CancellationToken cancellationToken)
    {
        return await _context.ParamTypes.ToListAsync(cancellationToken);
    }

    public IQueryable<ParamType> GetAllParamTypesQueryable(CancellationToken cancellationToken)
    {
        return _context.ParamTypes
            .AsNoTracking();
    }

    public async Task<ParamType?> GetByIdAsync(ParamTypeId id, CancellationToken cancellationToken)
    {
        return await _context.ParamTypes
       .Where(pt => pt.Id == id)
       .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<ParamType> AddParamTypeAsync(ParamType paramtype, CancellationToken cancellationToken)
    {
        await _context.ParamTypes.AddAsync(paramtype);
        await _context.SaveChangesAsync(cancellationToken);

        return paramtype;
    }

    public async Task UpdateParamTypeAsync(ParamType paramtype, CancellationToken cancellationToken)
    {
        _context.ParamTypes.Update(paramtype);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteParamTypeAsync(ParamType paramtype, CancellationToken cancellationToken)
    {
        _context.ParamTypes.Remove(paramtype);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<ParamType>> GetFilteredParamTypesAsync(GetAllParamTypesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        // apply filters
        var query = _context.ParamTypes.AsQueryable()
            .ApplyFilters(filters);

        // pagination
        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllParamTypesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.ParamTypes.AsQueryable()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<ParamType, bool>>> BuildFilters(GetAllParamTypesQuery request)
    {
        var filters = new List<Expression<Func<ParamType, bool>>>();

        if (request.TypeDefinitionId.Value != Guid.Empty)
        {
            var typeDefinitionId = TypeDefinitionId.Of(request.TypeDefinitionId.Value);
            filters.Add(pt => pt.TypeDefinitionId == typeDefinitionId);
        }

        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            filters.Add(pt => pt.Value.ToUpper().Equals(request.Value.ToUpper()));
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(pt => pt.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}