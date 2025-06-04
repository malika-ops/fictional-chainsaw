using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Products.Commands.PatchProduct;

public record PatchProductCommand : ICommand<Result<bool>>
{
    // The ID from the route
    public Guid ProductId { get; init; }

    // The optional fields to Product
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
}
