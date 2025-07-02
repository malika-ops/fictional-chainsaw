using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Dtos;

public record GetTaxRuleDetailsResponse
{
    /// <summary>
    /// Unique identifier of the tax rule detail.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid TaxRuleDetailsId { get; init; }

    /// <summary>
    /// Unique identifier of the corridor associated with the tax rule detail.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid CorridorId { get; init; }

    /// <summary>
    /// Unique identifier of the tax.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid TaxId { get; init; }

    /// <summary>
    /// Unique identifier of the service associated with the tax rule detail.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid ServiceId { get; init; }

    /// <summary>
    /// Rule indicating how the tax is applied. Possible values: "Fees", "Amount".
    /// </summary>
    /// <example>Fees</example>
    public ApplicationRule AppliedOn { get; init; }

    /// <summary>
    /// Indicates whether the tax rule detail is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}