using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class IdentityDocumentRepository : IIdentityDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public IdentityDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IdentityDocument>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.IdentityDocuments.ToListAsync(cancellationToken);
    }

    public async Task<IdentityDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.IdentityDocuments
            .FirstOrDefaultAsync(d => d.Id == IdentityDocumentId.Of(id), cancellationToken);
    }

    public async Task<IdentityDocument?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.IdentityDocuments
            .FirstOrDefaultAsync(d => d.Code == code, cancellationToken);
    }

    public async Task<IdentityDocument> AddAsync(IdentityDocument document, CancellationToken cancellationToken)
    {
        await _context.IdentityDocuments.AddAsync(document, cancellationToken);
        return document;
    }

    public Task UpdateAsync(IdentityDocument document, CancellationToken cancellationToken)
    {
        _context.IdentityDocuments.Update(document);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(IdentityDocument document, CancellationToken cancellationToken)
    {
        _context.IdentityDocuments.Remove(document);
        return Task.CompletedTask;
    }

    public async Task<List<IdentityDocument>> GetByCriteriaAsync(GetAllIdentityDocumentsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.IdentityDocuments
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(GetAllIdentityDocumentsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.IdentityDocuments
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private static List<Expression<Func<IdentityDocument, bool>>> BuildFilters(GetAllIdentityDocumentsQuery request)
    {
        var filters = new List<Expression<Func<IdentityDocument, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
            filters.Add(x => x.Code == request.Code);

        if (!string.IsNullOrWhiteSpace(request.Name))
            filters.Add(x => x.Name == request.Name);

        if (request.IsEnabled.HasValue)
            filters.Add(x => x.IsEnabled == request.IsEnabled);

        return filters;
    }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}