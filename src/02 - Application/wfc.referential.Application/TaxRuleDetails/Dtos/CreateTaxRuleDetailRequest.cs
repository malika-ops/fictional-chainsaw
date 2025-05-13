using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Dtos;
/// <summary>
/// Request DTO to create a new TaxRuleDetail.
/// </summary>
public record CreateTaxRuleDetailRequest
{
    public Guid? CorridorId { get; init; }
    public Guid? TaxId { get; init; }
    public Guid? ServiceId { get; init; }
    public ApplicationRule AppliedOn { get; init; }
}