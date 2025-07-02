using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Dtos;

public record ParamTypesResponse
{
    /// <summary>
    /// Unique identifier of the parameter type.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid ParamTypeId { get; init; }

    /// <summary>
    /// Value of the parameter type.
    /// </summary>
    /// <example>MAX_AMOUNT</example>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the parameter type is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Unique identifier of the type definition associated with this parameter type.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public TypeDefinitionId TypeDefinitionId { get; init; }
}