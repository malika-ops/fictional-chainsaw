namespace wfc.referential.Application.Sectors.Dtos;

public record SectorResponse
{
    public Guid SectorId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid CityId { get; init; }
    public string? CityName { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}