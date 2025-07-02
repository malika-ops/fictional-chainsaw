using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Dtos;

/// <summary>
/// Request DTO to fully update a TaxRuleDetail.
/// </summary>
public record UpdateTaxRuleDetailRequest
{
    public Guid CorridorId { get; init; }
    public Guid TaxId { get; init; }
    public Guid ServiceId { get; init; }
    public ApplicationRule? AppliedOn { get; init; }
    public bool IsEnabled { get; init; }
}