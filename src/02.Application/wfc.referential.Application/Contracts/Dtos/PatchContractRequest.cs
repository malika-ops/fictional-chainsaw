namespace wfc.referential.Application.Contracts.Dtos;

public record PatchContractRequest
{
    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>CTR002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the Partner ID. If omitted, Partner remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? PartnerId { get; init; }

    /// <summary>
    /// If provided, updates the start date. If omitted, start date remains unchanged.
    /// </summary>
    /// <example>2024-02-01</example>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// If provided, updates the end date. If omitted, end date remains unchanged.
    /// </summary>
    /// <example>2025-01-31</example>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}
