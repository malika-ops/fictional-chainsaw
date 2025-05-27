namespace wfc.referential.Application.Sectors.Dtos;

public record CreateSectorRequest
{
    /// <summary>Unique sector code.</summary>
    /// <example>SEC001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Sector name.</summary>
    /// <example>Downtown Sector</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>City identifier.</summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid CityId { get; init; }
}