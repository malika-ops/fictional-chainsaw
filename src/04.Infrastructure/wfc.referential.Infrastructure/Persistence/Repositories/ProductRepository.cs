using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ProductRepository : BaseRepository<Product, ProductId>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}
