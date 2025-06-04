using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Products.Commands.CreateProduct;

public record CreateProductCommand : ICommand<Result<Guid>>
{
    public Guid ProductId { get; init; } = default!;
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsEnabled { get; init; } = true;

}