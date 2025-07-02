namespace wfc.referential.Application.ServiceControles.Dtos;

public record GetFiltredServiceControlesRequest : FilterRequest
{
    /// <summary>
    /// Filter by ServiceId.
    /// <example>e.g. "b3b6a1e2-4c2d-4e7a-9c1a-2f3e4d5b6a7c"</example>
    /// </summary>
    public Guid? ServiceId { get; init; }

    /// <summary>
    /// Filter by ControleId.
    /// <example>e.g. "a1b2c3d4-e5f6-7890-ab12-cd34ef56gh78"</example>
    /// </summary>
    public Guid? ControleId { get; init; }

    /// <summary>
    /// Filter by ChannelId.
    /// <example>e.g. "123e4567-e89b-12d3-a456-426614174000"</example>
    /// </summary>
    public Guid? ChannelId { get; init; }
}