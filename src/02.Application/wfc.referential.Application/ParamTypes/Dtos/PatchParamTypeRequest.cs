namespace wfc.referential.Application.ParamTypes.Dtos;

public record PatchParamTypeRequest
{
    /// <summary> 
    /// Si fourni, met à jour la valeur du paramètre. Si omis, la valeur reste inchangée. 
    /// </summary>
    /// <example>Individuel</example>
    public string? Value { get; init; }

    /// <summary> 
    /// Si fourni, met à jour le type de définition associé. Si omis, l'association reste inchangée. 
    /// </summary>
    /// <example>50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1</example>
    public Guid? TypeDefinitionId { get; init; }

    /// <summary> 
    /// Si fourni, met à jour l'état d'activation. Si omis, l'état reste inchangé. 
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}