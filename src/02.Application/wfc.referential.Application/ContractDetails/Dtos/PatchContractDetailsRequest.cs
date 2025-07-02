namespace wfc.referential.Application.ContractDetails.Dtos;

public record PatchContractDetailsRequest
{
    /// <summary>
    /// The ID of the ContractDetails to patch.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid ContractDetailsId { get; init; }

    /// <summary>
    /// If provided, updates the Contract ID. If omitted, Contract remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? ContractId { get; init; }

    /// <summary>
    /// If provided, updates the Pricing ID. If omitted, Pricing remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? PricingId { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}
