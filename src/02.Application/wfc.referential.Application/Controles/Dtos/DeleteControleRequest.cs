using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Controles.Dtos;

public record DeleteControleRequest
{
    /// <summary>The GUID of the Controle to delete (route param).</summary>
    [Required]
    public Guid ControleId { get; init; }
}
