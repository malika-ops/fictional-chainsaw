namespace wfc.referential.Application.Sectors.Dtos;

public record PatchSectorRequest
{
    /// <summary>Unique sector code.</summary>
    public string? Code { get; init; }

    /// <summary>Sector name.</summary>
    public string? Name { get; init; }

    /// <summary>City identifier.</summary>
    public Guid? CityId { get; init; }

    /// <summary>Sector status (enabled/disabled).</summary>
    public bool? IsEnabled { get; init; }
}