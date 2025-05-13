namespace wfc.referential.Application.Tiers.Dtos;

public record TierResponse(
    Guid TierId,
    string Name,
    string Description,
    bool IsEnabled);
