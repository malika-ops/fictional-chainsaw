using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.CreateTax;

public record CreateTaxCommand : ICommand<Result<Guid>>
{
    public TaxId TaxId { get; init; } = TaxId.Of(Guid.NewGuid());
    public string Code { get; init; } = default!;
    public string CodeEn { get; init; } = default!;
    public string CodeAr { get; init; } = default!;
    public string Description { get; init; } = default!;
    public double? FixedAmount { get; init; }
    public double? Rate { get; init; }
    public bool IsEnabled { get; init; } = true;
}
