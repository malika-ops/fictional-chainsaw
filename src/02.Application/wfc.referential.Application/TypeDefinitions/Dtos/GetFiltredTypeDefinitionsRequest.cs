namespace wfc.referential.Application.TypeDefinitions.Dtos;

/// <summary>
/// Represents the request parameters for getting all typedefinitions.
/// </summary>
/// <example>
/// {
///     PageNumber = 1,
///     PageSize = 10,
///     Libelle = "Type Compte Support",
///     Description = "Type Compte Support (Commun ; Individuel)",
/// };
/// </example>
public record GetFiltredTypeDefinitionsRequest : FilterRequest
{
    /// <summary>Optional filter by libelle.</summary>
    public string? Libelle { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }
}