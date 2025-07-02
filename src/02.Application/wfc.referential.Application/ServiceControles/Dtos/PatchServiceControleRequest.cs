using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.ServiceControles.Dtos;

public record PatchServiceControleRequest
{
    /// <summary>
    /// Optional replacement ServiceId.
    /// <example>e1b2c3d4-e5f6-7890-ab12-cd34ef56gh78</example>
    /// </summary>
    public Guid? ServiceId { get; init; }

    /// <summary>
    /// Optional replacement ControleId.
    /// <example>f1234567-e89b-12d3-a456-426614174000</example>
    /// </summary>
    public Guid? ControleId { get; init; }

    /// <summary>
    /// Optional replacement ChannelId (ParamType).
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    /// </summary>
    public Guid? ChannelId { get; init; }

    /// <summary>
    /// Optional new execution order.
    /// <example>2</example>
    /// </summary>
    public int? ExecOrder { get; init; }

    /// <summary>
    /// Optional enabled flag.
    /// <example>true</example>
    /// </summary>
    public bool? IsEnabled { get; init; }
}
