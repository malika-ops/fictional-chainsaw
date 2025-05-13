using wfc.referential.Application.Countries.Dtos;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record MonetaryZoneResponse( 
    Guid MonetaryZoneId,
    string Code,
    string Name,
    string Description,
    bool IsEnabled,
    List<GetCountriesResponce>? Countries
    );
