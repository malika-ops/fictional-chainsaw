using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record UpdateMonetaryZoneRequest
{
    /// <summary>
    /// The ID of the MonetaryZone to update.
    /// must be unique.
    /// </summary>
    ///<example>United States Dollar</example>
    [Required]
    public Guid MonetaryZoneId { get; init; }

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

    /// <summary> 
    /// If provided, updates the status. If omitted, description remains unchanged. 
    /// </summary>
    public bool IsEnabled { get; init; }
}
