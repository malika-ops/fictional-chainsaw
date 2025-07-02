using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.ServiceControles.Dtos;

public record GetServiceControleByIdRequest
{
    /// <summary>Service-Controle ID (GUID).</summary>
    [Required]
    public Guid ServiceControleId { get; init; }
}
