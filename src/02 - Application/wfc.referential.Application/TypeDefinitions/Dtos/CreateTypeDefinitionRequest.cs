using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.TypeDefinitions.Dtos;
public record CreateTypeDefinitionRequest
{
    /// <summary>Libelle.</summary>
    /// <example>Type Compte Support</example>
    public string Libelle { get; init; } = string.Empty;

    /// <summary>Description.</summary>
    /// <example>Type Compte Support (Commun ; Individuel)</example>
    public string Description { get; init; } = string.Empty;
}