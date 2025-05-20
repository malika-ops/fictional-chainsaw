using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record PatchMonetaryZoneRequest
{
    /// <summary>
    /// The string representation of the MonetaryZone's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    [Required]
    public Guid MonetaryZoneId { get; init; }

    /// <summary> 
    /// If provided, updates the code. If omitted, code remains unchanged. 
    /// </summary>
    public string? Code { get; init; }

    /// <summary> 
    /// If provided, updates the name. If omitted, name remains unchanged. 
    /// </summary>
    public string? Name { get; init; }

    /// <summary> 
    /// If provided, updates the description. If omitted, description remains unchanged. 
    /// </summary>
    public string? Description { get; init; }

    /// <summary> 
    /// If provided, updates the status. If omitted, description remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }
}
