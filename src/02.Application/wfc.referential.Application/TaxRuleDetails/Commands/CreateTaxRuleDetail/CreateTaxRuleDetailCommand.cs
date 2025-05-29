using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;

public record CreateTaxRuleDetailCommand : ICommand<Result<Guid>>
{
    public TaxRuleDetailsId TaxRuleDetailsId { get; init; } = TaxRuleDetailsId.Of(Guid.NewGuid());
    public CorridorId CorridorId { get; init; } = default!;
    public TaxId TaxId { get; init; } = default!;
    public ServiceId ServiceId { get; init; } = default!;

    /// <summary>
    /// Specifies where the tax is applied, e.g. "Amount" or "Fees"
    /// </summary>
    public ApplicationRule AppliedOn { get; init; } = default!;

    /// <summary>
    /// Related service identifier
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}
