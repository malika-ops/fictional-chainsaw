using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Cities.Dtos;

public record GetAllCitiesResponse(
    Guid CityId,
    string Code,
    string Name,
    string Abbreviation,
    string TimeZone,
    string TaxZone,
    Guid RegionId,
    bool IsEnabled,
    List<SectorResponse> Sectors
    );
