using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Contracts.Dtos;

public record CreateContractRequest
{
    /// <summary>
    /// A unique code identifier for the Contract.
    /// </summary>
    /// <example>CTR001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the Partner associated with this contract.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// The start date of the contract.
    /// </summary>
    /// <example>2024-01-01</example>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// The end date of the contract.
    /// </summary>
    /// <example>2024-12-31</example>
    public DateTime EndDate { get; init; }
}
