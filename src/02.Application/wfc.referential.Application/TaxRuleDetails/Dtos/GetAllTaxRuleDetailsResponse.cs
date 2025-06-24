using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Dtos;

/// <summary>
/// Response DTO representing a single TaxRuleDetail in a list.
/// </summary>
public record GetFiltredTaxRuleDetailsResponse
{
    public Guid TaxRuleDetailsId { get; init; }
    public Guid CorridorId { get; init; }
    public Guid TaxId { get; init; }
    public Guid ServiceId { get; init; }
    public ApplicationRule AppliedOn { get; init; }
    public bool IsEnabled { get; init; }
}