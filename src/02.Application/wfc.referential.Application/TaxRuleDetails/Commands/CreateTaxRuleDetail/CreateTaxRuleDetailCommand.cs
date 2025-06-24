using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;

public record CreateTaxRuleDetailCommand : ICommand<Result<Guid>>
{
    public Guid CorridorId { get; init; }
    public Guid TaxId { get; init; } = default!;
    public Guid ServiceId { get; init; } = default!;

    /// <summary>
    /// Specifies where the tax is applied, e.g. "Amount" or "Fees"
    /// </summary>
    public ApplicationRule AppliedOn { get; init; } = default!;

    /// <summary>
    /// Related service identifier
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}
