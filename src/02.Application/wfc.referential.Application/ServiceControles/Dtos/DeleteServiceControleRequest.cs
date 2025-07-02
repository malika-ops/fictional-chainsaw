using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.ServiceControles.Dtos;

public record DeleteServiceControleRequest
{
    /// <summary>GUID of the Service-Controle to delete (route).</summary>
    /// <example>f86af0e0-d949-40bc-9c08-6e4627e8b79b</example>
    [Required]
    public Guid ServiceControleId { get; init; }
}
