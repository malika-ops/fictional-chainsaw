namespace wfc.referential.Application.ParamTypes.Dtos;

public record UpdateParamTypeRequest
{
    /// <summary>
    /// La nouvelle valeur du paramètre.
    /// </summary>
    /// <example>Individuel</example>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// L'identifiant unique du type de définition auquel ce paramètre est associé.
    /// </summary>
    /// <example>50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1</example>
    public Guid TypeDefinitionId { get; init; }

    /// <summary>
    /// Indique si le paramètre est activé ou désactivé.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}