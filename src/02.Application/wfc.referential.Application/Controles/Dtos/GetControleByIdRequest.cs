using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Controles.Dtos;

public record GetControleByIdRequest
{
    /// <summary>Controle ID (route).</summary>
    [Required]
    public Guid ControleId { get; init; }
}
