using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.AgencyTiers.Dtos;

public record DeleteAgencyTierRequest
{
    /// <summary>
    /// The GUID of the Agency-Tier to delete (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    [Required]
    public Guid AgencyTierId { get; init; }
}
