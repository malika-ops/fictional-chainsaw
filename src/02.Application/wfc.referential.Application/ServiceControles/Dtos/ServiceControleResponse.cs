using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.Application.ServiceControles.Dtos;

public record ServiceControleResponse
{
    /// <summary>
    /// Unique identifier of the ServiceControle.
    /// <example>b3b6a1e2-4c2d-4e7a-9c1a-2f3e4d5b6a7c</example>
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// identifier of the related Service.
    /// <example>e1b2c3d4-e5f6-7890-ab12-cd34ef56gh78</example>
    /// </summary>
    public Guid ServiceId { get; init; }

    /// <summary>
    /// identifier of the related Controle.
    /// <example>f1234567-e89b-12d3-a456-426614174000</example>
    /// </summary>
    public Guid ControleId { get; init; }

    /// <summary>
    /// Execution order (integer ≥ 0).
    /// <example>1</example>
    /// </summary>
    public int ExecOrder { get; init; }

    /// <summary>
    /// Indicates if the ServiceControle is enabled.
    /// <example>true</example>
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Channel information.
    /// </summary>
    public ParamTypesResponse Channel { get; init; } = default!;
}
