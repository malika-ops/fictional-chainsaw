namespace wfc.referential.Application.ParamTypes.Dtos;

/// <summary>
/// Représente les paramètres de requête pour obtenir tous les paramètres.
/// </summary>
/// <example>
/// {
///     PageNumber = 1,
///     PageSize = 10,
///     Value = "Commun",
///     TypeDefinitionId = "50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1"
/// };
/// </example>
public record GetFiltredParamTypesRequest : FilterRequest
{

    /// <summary>Filtre optionnel par valeur.</summary>
    /// <example>Commun</example>
    public string? Value { get; init; }

    /// <summary>Filtre obligatoire par ID de type de définition.</summary>
    /// <example>50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1</example>
    public Guid? TypeDefinitionId { get; init; }
}