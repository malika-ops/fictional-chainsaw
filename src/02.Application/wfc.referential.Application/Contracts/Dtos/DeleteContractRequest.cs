namespace wfc.referential.Application.Contracts.Dtos;

public record DeleteContractRequest
{
    /// <summary>
    /// The ID of the Contract to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid ContractId { get; init; }
}