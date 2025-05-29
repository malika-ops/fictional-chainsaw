using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;
/// <summary>
/// Command to partially update a TaxRuleDetails entity.
/// </summary>
public record PatchTaxRuleDetailCommand : ICommand<Result<Guid>>
{
    /// <summary>
    /// The ID of the TaxRuleDetails to update.
    /// </summary>
    public Guid TaxRuleDetailsId { get; init; }

    /// <summary>
    /// Optional CorridorId to update.
    /// </summary>
    public CorridorId? CorridorId { get; init; }

    /// <summary>
    /// Optional TaxId to update.
    /// </summary>
    public TaxId? TaxId { get; init; }

    /// <summary>
    /// Optional ServiceId to update.
    /// </summary>
    public ServiceId? ServiceId { get; init; }

    /// <summary>
    /// Optional AppliedOn value to update (e.g., "Amount" or "Fees").
    /// </summary>
    public ApplicationRule? AppliedOn { get; init; }


    /// <summary>
    /// Optional IsEnabled flag to update.
    /// </summary>
    public bool? IsEnabled { get; init; }
}
