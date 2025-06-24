using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Cities.Dtos;

public record GetCitiyResponse(
    Guid CityId,
    string Code,
    string Name,
    string Abbreviation,
    string TimeZone,
    Guid RegionId,
    bool IsEnabled,
    List<SectorResponse> Sectors
    );
