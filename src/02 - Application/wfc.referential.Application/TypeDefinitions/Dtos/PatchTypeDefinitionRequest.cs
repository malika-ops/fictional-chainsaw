namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record PatchTypeDefinitionRequest
{
    /// <summary>
    /// The string representation of the Type definition's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid TypeDefinitionId { get; init; }

    /// <summary> 
    /// If provided, updates the libelle. If omitted, libelle remains unchanged. 
    /// </summary>
    public string? Libelle { get; init; }

    /// <summary> 
    /// If provided, updates the description. If omitted, description remains unchanged. 
    /// </summary>
    public string? Description { get; init; }

    /// <summary> 
    /// If provided, updates the enabled status. If omitted, status remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }
}