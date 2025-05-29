using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Taxes.Commands.DeleteTax;

public record DeleteTaxCommand : ICommand<Result<bool>>
{
    public Guid TaxId { get; init; }
}
