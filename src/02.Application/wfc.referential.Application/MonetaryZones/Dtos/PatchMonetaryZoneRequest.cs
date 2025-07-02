using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record PatchMonetaryZoneRequest
{
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
