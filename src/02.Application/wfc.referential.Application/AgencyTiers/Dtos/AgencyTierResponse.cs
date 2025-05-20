namespace wfc.referential.Application.AgencyTiers.Dtos;

public record AgencyTierResponse(
    Guid AgencyTierId,
    Guid AgencyId,
    Guid TierId,
    string Code,
    bool IsEnabled
);