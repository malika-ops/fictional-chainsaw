
namespace wfc.referential.Application.ContractDetails.Dtos;

public record UpdateContractDetailsRequest
{
    /// <summary>
    /// The ID of the Contract associated with this ContractDetails.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ContractId { get; init; }

    /// <summary>
    /// The ID of the Pricing associated with this ContractDetails.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid PricingId { get; init; }

    /// <summary>
    /// Whether the ContractDetails is enabled or not.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}