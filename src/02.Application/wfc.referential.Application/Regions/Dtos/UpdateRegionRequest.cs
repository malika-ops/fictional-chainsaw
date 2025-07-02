using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Dtos;

public record UpdateRegionRequest
{
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
    public Guid CountryId { get; set; }
}
