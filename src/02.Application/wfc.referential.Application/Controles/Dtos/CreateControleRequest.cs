using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Controles.Dtos;

public record CreateControleRequest
{
    /// <summary>Unique code of the Controle.</summary>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>Name of the Controle.</summary>
    [Required]
    public string Name { get; init; } = string.Empty;
}
