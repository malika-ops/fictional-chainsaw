namespace wfc.referential.Application.TaxRuleDetails.Dtos;


/// <summary>
/// Response DTO after creating a TaxRuleDetail.
/// </summary>
public record CreateTaxRuleDetailResponse
{
    public Guid TaxRuleDetailsId { get; init; }
}