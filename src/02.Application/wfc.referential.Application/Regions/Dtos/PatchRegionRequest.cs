using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Regions.Dtos;

public record PatchRegionRequest
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
    /// If provided, updates the status. If omitted, description remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }

    /// <summary> 
    /// If provided, updates the country id. If omitted, description remains unchanged. 
    /// </summary>
    public Guid? CountryId { get; init; }
}
