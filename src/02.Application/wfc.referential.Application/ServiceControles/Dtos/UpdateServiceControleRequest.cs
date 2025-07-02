using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.ServiceControles.Dtos;

public record UpdateServiceControleRequest
{
    /// <summary>Route param — ID of the Service-Controle mapping to update.</summary>
    /// <example>4e1d1c11-7f3b-4e28-a9c2-79cfb4e4995f</example>
    [Required] public Guid ServiceControleId { get; init; }

    /// <summary>ID of the Service.</summary>
    /// <example>d1ee58ec-e4de-4b71-86d3-8d2c8d9e1d3f</example>
    [Required] public Guid ServiceId { get; init; }

    /// <summary>ID of the Controle.</summary>
    /// <example>7c2b325c-be10-4ad5-8036-1134e1f042ac</example>
    [Required] public Guid ControleId { get; init; }

    /// <summary>ID of the execution Channel (ParamType).</summary>
    /// <example>2f9b77c0-f936-4dcc-bce8-d199d1447e57</example>
    [Required] public Guid ChannelId { get; init; }

    /// <summary>Execution order (0 = first).</summary>
    /// <example>1</example>
    public int ExecOrder { get; init; }

    /// <summary>Status flag (default = true).</summary>
    public bool IsEnabled { get; init; } = true;
}
