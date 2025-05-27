namespace wfc.referential.Application.Sectors.Dtos;

public record UpdateSectorRequest
{
    /// <summary>Sector GUID (from route).</summary>
    public Guid SectorId { get; init; }

    /// <summary>Unique sector code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Sector name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>City identifier.</summary>
    public Guid CityId { get; init; }

    /// <summary>Sector status (enabled/disabled).</summary>
    public bool IsEnabled { get; init; } = true;
}