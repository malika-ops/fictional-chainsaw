using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;

/// <summary>
/// Command to update all details of a TaxRuleDetail entity.
/// </summary>
public record UpdateTaxRuleDetailCommand : ICommand<Result<Guid>>
{
    public Guid TaxRuleDetailsId { get; init; }

    public CorridorId CorridorId { get; init; } = default!;
    public TaxId TaxId { get; init; } = default!;
    public ServiceId ServiceId { get; init; } = default!;

    public ApplicationRule? AppliedOn { get; init; }

    public bool IsEnabled { get; init; }
}