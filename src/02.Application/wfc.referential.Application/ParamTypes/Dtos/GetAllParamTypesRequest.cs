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
public record GetAllParamTypesRequest
{
    /// <summary>Numéro de page.</summary>
    /// <example>1</example>
    /// <value>Le numéro de page par défaut est 1</value>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Taille de page (par défaut = 10).</summary>
    /// <remarks>La taille de page par défaut est 10</remarks>
    /// <value>10</value>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filtre optionnel par valeur.</summary>
    /// <example>Commun</example>
    public string? Value { get; init; }

    /// <summary>Filtre optionnel par état d'activation.</summary>
    /// <example>true</example>
    public bool? IsEnabled { get; init; }

    /// <summary>Filtre obligatoire par ID de type de définition.</summary>
    /// <example>50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1</example>
    public Guid TypeDefinitionId { get; init; }
}