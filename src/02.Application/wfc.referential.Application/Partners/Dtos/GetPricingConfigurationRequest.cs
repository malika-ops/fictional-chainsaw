using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Partners.Dtos;

public record GetPricingConfigurationRequest
{
    [Required]
    public Guid PartnerId { get; init; }

    [Required]
    public Guid ServiceId { get; init; }

    [Required]
    public Guid CorridorId { get; init; }

    [Required]
    public Guid AffiliateId { get; init; }

    [Required]
    public string Channel { get; init; } = string.Empty;

    [Required]
    public decimal Amount { get; init; }
}
