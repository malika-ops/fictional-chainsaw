using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Controles.Dtos;

public record UpdateControleRequest
{
    /// <summary>Unique code of the Controle.</summary>
    [Required] public string Code { get; init; } = string.Empty;

    /// <summary>Name of the Controle.</summary>
    [Required] public string Name { get; init; } = string.Empty;

    /// <summary>Whether the Controle is enabled.</summary>
    public bool IsEnabled { get; init; } = true;
}
