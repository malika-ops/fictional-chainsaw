namespace wfc.referential.Application.ContractDetails.Dtos;

public record GetContractDetailsResponse
{
    /// <summary>
    /// Unique identifier of the contract details.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid ContractDetailsId { get; init; }

    /// <summary>
    /// Unique identifier of the contract.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid ContractId { get; init; }

    /// <summary>
    /// Unique identifier of the pricing associated with the contract details.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid PricingId { get; init; }

    /// <summary>
    /// Indicates whether the contract details are enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}
