namespace wfc.referential.Application.TaxRuleDetails.Dtos;

/// <summary>
/// Request DTO to delete a TaxRuleDetail.
/// </summary>
public record DeleteTaxRuleDetailRequest
{
    public Guid TaxRuleDetailId { get; init; }
}