using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Dtos;
/// <summary>
/// Request DTO to get paginated list of TaxRuleDetails with optional filters.
/// </summary>
public record GetAllTaxRuleDetailsRequest
{
    public Guid? CorridorId { get; init; }
    public Guid? TaxId { get; init; }
    public Guid? ServiceId { get; init; }

    public ApplicationRule? AppliedOn { get; init; }
    public bool? IsEnabled { get; init; }

    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}