using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.AgencyTiers.Dtos;

public record UpdateAgencyTierRequest
{
    /// <summary>ID of the AgencyTier to update (route).</summary>
    [Required] public Guid AgencyTierId { get; init; }

    /// <summary>ID of the linked Agency.</summary>
    [Required] public Guid AgencyId { get; init; }

    /// <summary>ID of the linked Tier.</summary>
    [Required] public Guid TierId { get; init; }

    /// <summary>Unique code used for the Tier inside the Agency.</summary>
    [Required] public string Code { get; init; } = string.Empty;

    /// <summary>Password used for authentication with the Tier.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>Whether the link is enabled (default = true).</summary>
    public bool IsEnabled { get; init; } = true;
}
