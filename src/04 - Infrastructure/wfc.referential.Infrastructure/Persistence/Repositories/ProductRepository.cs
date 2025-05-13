using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Products.Queries.GetAllProducts;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        return await _context.Products.ToListAsync(cancellationToken);
    }
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(Product => Product.Id == ProductId.Of(id))
            .Include(Product => Product.Services)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Product?> GetByCodeAsync(string ProductCode, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(Product => Product.Code == ProductCode)
            .Include(Product => Product.Services)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Product> AddProductAsync(Product Product, CancellationToken cancellationToken)
    {
        await _context.Products.AddAsync(Product);
        await _context.SaveChangesAsync(cancellationToken);

        return Product;
    }

    public async Task UpdateProductAsync(Product Product, CancellationToken cancellationToken)
    {
        _context.Products.Update(Product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteProductAsync(Product Product, CancellationToken cancellationToken)
    {

        _context.Products.Remove(Product);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<Product>> GetProductsByCriteriaAsync(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Products
            .Include(Product => Product.Services)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Products
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<Product, bool>>> BuildFilters(GetAllProductsQuery request)
    {
        var filters = new List<Expression<Func<Product, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(reg => reg.Code!.Equals(request.Code, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(reg => reg.Name!.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(reg => reg.IsEnabled == request.IsEnabled);
        }

        return filters;
    }

    public async Task<List<Service>> GetServicesByProductIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _context.Services
            .Where(r => r.ProductId == ProductId.Of(productId))
            .ToListAsync();
    }
}
