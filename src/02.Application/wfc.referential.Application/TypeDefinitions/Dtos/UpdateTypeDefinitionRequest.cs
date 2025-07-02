namespace wfc.referential.Application.TypeDefinitions.Dtos;

public record UpdateTypeDefinitionRequest
{
    /// <summary>
    /// Le nouveau libellé du type de paramètre.
    /// </summary>
    /// <example>Type Compte Support</example>
    public string Libelle { get; init; } = string.Empty;

    /// <summary>
    /// La nouvelle description détaillée du type de paramètre.
    /// </summary>
    /// <example>Type Compte Support (Commun ; Individuel)</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Indique si le type de paramètre est activé ou désactivé.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}