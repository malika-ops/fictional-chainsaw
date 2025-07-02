namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record PatchTypeDefinitionRequest
{
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