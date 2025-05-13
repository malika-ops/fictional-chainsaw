using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record CreateMonetaryZoneRequest
{
    /// <summary>
    /// A short code for the MonetaryZone.  
    /// must be unique.
    /// </summary>
    /// <example>EU</example>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// A human-readable name.  
    /// </summary>
    ///<example>Europe</example>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// A brief descriptive text.  
    /// </summary>
    ///<example>Europe Monetary Zone</example>
    public string Description { get; init; } = string.Empty;
}
