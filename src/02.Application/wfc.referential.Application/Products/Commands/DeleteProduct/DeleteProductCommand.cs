using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand : ICommand<Result<bool>>
{
    public Guid ProductId { get; init; }
}