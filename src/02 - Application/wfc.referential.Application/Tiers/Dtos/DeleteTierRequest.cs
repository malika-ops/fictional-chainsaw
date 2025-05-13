using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Tiers.Dtos;

public record DeleteTierRequest
{
    /// <summary>Tier ID (route param).</summary>
    /// <example>3a60f88e-5ee0-4389-8fc6-c2b1903cf30d</example>
    [Required] public Guid TierId { get; init; }
}
