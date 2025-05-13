using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.RegionManagement.Dtos;

public record GetAllRegionsResponse(
    Guid RegionId,
    string Code,
    string Name,
    bool IsEnabled,
    CountryId CountryId,
    List<GetAllCitiesResponse>? Cities
    );
