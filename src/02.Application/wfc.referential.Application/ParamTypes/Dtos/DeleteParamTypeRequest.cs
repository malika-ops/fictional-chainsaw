namespace wfc.referential.Application.ParamTypes.Dtos;
public record DeleteParamTypeRequest
{
    /// <summary>
    /// The string representation of the ParamType GUID.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid ParamTypeId { get; init; }
}
