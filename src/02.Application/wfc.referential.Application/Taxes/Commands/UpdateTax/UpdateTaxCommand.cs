using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Taxes.Commands.UpdateTax;

/// <summary>
/// Command to update all details of a tax entity.
/// </summary>
public record UpdateTaxCommand : ICommand<Result<bool>>
{
    public Guid TaxId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string CodeEn { get; init; } = string.Empty;
    public string CodeAr { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double FixedAmount { get; init; }
    public double Rate { get; init; }
    public bool IsEnabled { get; init; }
}
