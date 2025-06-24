using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;

/// <summary>
/// Command to update all details of a TaxRuleDetail entity.
/// </summary>
public record UpdateTaxRuleDetailCommand : ICommand<Result<bool>>
{
    public Guid TaxRuleDetailsId { get; init; }

    public Guid CorridorId { get; init; } = default!;
    public Guid TaxId { get; init; } = default!;
    public Guid ServiceId { get; init; } = default!;

    public ApplicationRule? AppliedOn { get; init; }

    public bool IsEnabled { get; init; }
}