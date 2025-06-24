using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;
/// <summary>
/// Command to partially update a TaxRuleDetails entity.
/// </summary>
public record PatchTaxRuleDetailCommand : ICommand<Result<bool>>
{
    /// <summary>
    /// The ID of the TaxRuleDetails to update.
    /// </summary>
    public Guid TaxRuleDetailsId { get; init; }

    /// <summary>
    /// Optional CorridorId to update.
    /// </summary>
    public Guid? CorridorId { get; init; }

    /// <summary>
    /// Optional TaxId to update.
    /// </summary>
    public Guid? TaxId { get; init; }

    /// <summary>
    /// Optional ServiceId to update.
    /// </summary>
    public Guid? ServiceId { get; init; }

    /// <summary>
    /// Optional AppliedOn value to update (e.g., "Amount" or "Fees").
    /// </summary>
    public ApplicationRule? AppliedOn { get; init; }


    /// <summary>
    /// Optional IsEnabled flag to update.
    /// </summary>
    public bool? IsEnabled { get; init; }
}
