using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery : IQuery<GetProdcutsResponse>
{
    public Guid ProductId { get; init; }
}
