namespace wfc.referential.Application.Regions.Dtos;
public record CreateRegionRequest
{
    /// <summary>Region Code.</summary>
    /// <example>110</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Region Name.</summary>
    /// <example>Casablanca-Settat</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Region Country.</summary>
    /// <example>50ed04f5-d16b-49c6-af46-b3ea7dfb8cb1</example>
    public Guid CountryId { get; init; }
}
