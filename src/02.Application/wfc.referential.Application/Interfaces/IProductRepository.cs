using wfc.referential.Application.Products.Queries.GetAllProducts;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> GetByCodeAsync(string ProductCode, CancellationToken cancellationToken);
    Task<Product> AddProductAsync(Product Product, CancellationToken cancellationToken);
    Task UpdateProductAsync(Product Product, CancellationToken cancellationToken);
    Task DeleteProductAsync(Product Product, CancellationToken cancellationToken);
    Task<List<Product>> GetProductsByCriteriaAsync(GetAllProductsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllProductsQuery request, CancellationToken cancellationToken);
    Task<List<Service>> GetServicesByProductIdAsync(Guid productId, CancellationToken cancellationToken);


}
