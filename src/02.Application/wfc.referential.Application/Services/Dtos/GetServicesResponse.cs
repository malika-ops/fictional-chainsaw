namespace wfc.referential.Application.Services.Dtos;

public record GetServicesResponse
{
    /// <summary>
    /// Unique identifier of the service.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid ServiceId { get; init; }

    /// <summary>
    /// Unique code of the service.
    /// </summary>
    /// <example>SVC001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the service.
    /// </summary>
    /// <example>Money Transfer</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the service is enabled ("true" or "false" as string).
    /// </summary>
    /// <example>true</example>
    public string IsEnabled { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the product associated with the service.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid ProductId { get; init; }
}