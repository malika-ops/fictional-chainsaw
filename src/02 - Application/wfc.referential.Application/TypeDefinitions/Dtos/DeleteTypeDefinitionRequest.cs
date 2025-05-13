namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record DeleteTypeDefinitionRequest
{
    /// <summary>
    /// The string representation of the TypeDefinition GUID.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid TypeDefinitionId { get; init; }
}
