namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record GetTypeDefinitionsResponse
{
    /// <summary>
    /// Unique identifier of the type definition.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid TypeDefinitionId { get; init; }

    /// <summary>
    /// Label or name of the type definition.
    /// </summary>
    /// <example>DocumentType</example>
    public string Libelle { get; init; }

    /// <summary>
    /// Description of the type definition.
    /// </summary>
    /// <example>Defines the type of document used for identification</example>
    public string Description { get; init; }

    /// <summary>
    /// Indicates whether the type definition is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}