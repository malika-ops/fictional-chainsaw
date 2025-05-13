using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class TypeDefinitionRepository : ITypeDefinitionRepository
{
    private readonly ApplicationDbContext _context;

    public TypeDefinitionRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<TypeDefinition>> GetAllTypeDefinitionsAsync(CancellationToken cancellationToken)
    {
        return await _context.TypeDefinitions.ToListAsync(cancellationToken);
    }

    public IQueryable<TypeDefinition> GetAllTypeDefinitionsQueryable(CancellationToken cancellationToken)
    {
        return _context.TypeDefinitions
            .AsNoTracking();
    }

    public async Task<TypeDefinition?> GetByIdAsync(TypeDefinitionId id, CancellationToken cancellationToken)
    {
        return await _context.TypeDefinitions
       .Where(tf => tf.Id == id)
       .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<TypeDefinition> AddTypeDefinitionAsync(TypeDefinition typeDefinition, CancellationToken cancellationToken)
    {
        await _context.TypeDefinitions.AddAsync(typeDefinition);
        await _context.SaveChangesAsync(cancellationToken);

        return typeDefinition;
    }

    public async Task UpdateTypeDefinitionAsync(TypeDefinition typeDefinition, CancellationToken cancellationToken)
    {
        _context.TypeDefinitions.Update(typeDefinition);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTypeDefinitionAsync(TypeDefinition typeDefinition, CancellationToken cancellationToken)
    {
        _context.TypeDefinitions.Remove(typeDefinition);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // new pagination methods
    public async Task<List<TypeDefinition>> GetFilteredTypeDefinitionsAsync(GetAllTypeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        // apply filters
        var query = _context.TypeDefinitions
            .AsQueryable()
            .ApplyFilters(filters);

        // pagination
        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllTypeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.TypeDefinitions.AsQueryable()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<TypeDefinition, bool>>> BuildFilters(GetAllTypeDefinitionsQuery request)
    {
        var filters = new List<Expression<Func<TypeDefinition, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Libelle))
        {
            filters.Add(td => td.Libelle.ToUpper().Equals(request.Libelle.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            filters.Add(td => td.Description.ToUpper().Equals(request.Description.ToUpper()));
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(td => td.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}