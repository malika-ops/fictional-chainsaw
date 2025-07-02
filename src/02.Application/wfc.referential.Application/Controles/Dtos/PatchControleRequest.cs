using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Controles.Dtos;

public record PatchControleRequest
{
    /// <summary>Controle ID (route).</summary>
    [Required] public Guid ControleId { get; init; }

    /// <summary>Optional new Code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional new Name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional enabled flag.</summary>
    public bool? IsEnabled { get; init; }
}
