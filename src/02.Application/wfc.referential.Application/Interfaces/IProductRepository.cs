using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IProductRepository : IRepositoryBase<Product, ProductId>
{
}
