namespace wfc.referential.Application.AgencyTiers.Dtos;

public record AgencyTierResponse
{
    public Guid AgencyTierId { get; init; }
    public Guid AgencyId { get; init; }
    public Guid TierId { get; init; }
    public string Code { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}