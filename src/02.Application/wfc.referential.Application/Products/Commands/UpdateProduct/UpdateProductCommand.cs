using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : ICommand<Result<bool>>
{
    public Guid ProductId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
}
