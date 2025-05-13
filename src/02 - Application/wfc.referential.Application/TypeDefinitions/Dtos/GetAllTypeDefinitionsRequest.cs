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
public record GetAllTypeDefinitionsRequest
{
    /// <summary>Page number.</summary>
    /// <example>The default page number is 1</example>
    /// <value>The page number</value>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size (default = 10).</summary>
    /// <remarks>The default page size is 10</remarks>
    /// <value>The page size</value>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by libelle.</summary>
    public string? Libelle { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}