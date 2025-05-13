using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Dtos;

public record UpdateRegionRequest
{
    /// <summary>
    /// The string representation of the Region's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid RegionId { get; set; }

    /// <summary> 
    /// The code of the region. 
    /// </summary>
    public string Code { get; set; } =string.Empty;

    /// <summary> 
    /// The name of the region. 
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 
    /// The status of the region. 
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary> 
    /// The country ID associated with the region. 
    /// </summary>
    public CountryId CountryId { get; set; }
}
