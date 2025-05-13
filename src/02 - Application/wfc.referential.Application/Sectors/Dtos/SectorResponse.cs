namespace wfc.referential.Application.Sectors.Dtos;

public record SectorResponse(
    Guid SectorId,
    string Code,
    string Name,
    Guid CityId,
    string CityName,
    bool IsEnabled);