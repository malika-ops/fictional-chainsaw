using System.Linq.Expressions;
using BuildingBlocks.Core.Abstraction.Repositories;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Services.Queries.GetAllServices;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ServiceRepository : BaseRepository<Service, ServiceId> , IServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Service>> GetAllServicesAsync(CancellationToken cancellationToken)
    {
        return await _context.Services.ToListAsync(cancellationToken);
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Services
            .Where(s => s.Id == ServiceId.Of(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Service?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.Services
            .Where(s => s.Code == code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Service> AddServiceAsync(Service service, CancellationToken cancellationToken)
    {
        await _context.Services.AddAsync(service, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return service;
    }

    public async Task UpdateServiceAsync(Service service, CancellationToken cancellationToken)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteServiceAsync(Service service, CancellationToken cancellationToken)
    {
        _context.Services.Remove(service);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Service>> GetServicesByCriteriaAsync(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Services
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Services
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<Service, bool>>> BuildFilters(GetAllServicesQuery request)
    {
        var filters = new List<Expression<Func<Service, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
            filters.Add(s => s.Code!.Equals(request.Code, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.Name))
            filters.Add(s => s.Name!.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (request.IsEnabled.HasValue)
            filters.Add(s => s.IsEnabled == request.IsEnabled);

        return filters;
    }
}