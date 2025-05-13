using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.AgencyTiers.Dtos;

public record CreateAgencyTierRequest
{
    /// <summary>The Agency GUID.</summary>
    [Required] public Guid AgencyId { get; init; }

    /// <summary>The Tier GUID.</summary>
    [Required] public Guid TierId { get; init; }

    /// <summary>Unique code for this Agency–Tier couple.</summary>
    [Required] public string Code { get; init; } = string.Empty;

    /// <summary>Optional password.</summary>
    public string? Password { get; init; }

}
