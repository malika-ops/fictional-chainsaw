namespace wfc.referential.Application.ParamTypes.Dtos;
public record CreateParamTypeRequest
{
    /// <summary>Value.</summary>
    /// <example>Commun</example>
    public string Value { get; init; } = string.Empty;

    /// <summary>ParamType TypeDefinition.</summary>
    /// <example>50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1</example>
    public Guid TypeDefinitionId { get; init; }
}