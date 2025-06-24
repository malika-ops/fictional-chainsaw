using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Regions.Dtos;

public record PatchRegionRequest
{
    /// <summary>
    /// The string representation of the Region's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid RegionId { get; init; }

    /// <summary> 
    /// If provided, updates the code. If omitted, code remains unchanged. 
    /// </summary>
    public string? Code { get; init; }

    /// <summary> 
    /// If provided, updates the name. If omitted, name remains unchanged. 
    /// </summary>
    public string? Name { get; init; }

    /// <summary> 
    /// If provided, updates the status. If omitted, description remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }

    /// <summary> 
    /// If provided, updates the country id. If omitted, description remains unchanged. 
    /// </summary>
    public Guid? CountryId { get; init; }
}
