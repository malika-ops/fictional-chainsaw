using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.RegionManagement.Dtos;

public record GetFiltredRegionsResponse(
    Guid RegionId,
    string Code,
    string Name,
    bool IsEnabled,
    Guid CountryId,
    List<GetCitiyResponse>? Cities
    );
