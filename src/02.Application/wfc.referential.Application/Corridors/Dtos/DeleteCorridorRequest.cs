namespace wfc.referential.Application.Corridors.Dtos;

public record DeleteCorridorRequest
{
    /// <summary>
    /// The string representation of the Regions GUID.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid CorridorId { get; init; }
}
