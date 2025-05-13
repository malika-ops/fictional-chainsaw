using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Tiers.Dtos;

public record PatchTierRequest
{
    /// <summary>Tier identifier (route parameter).</summary>
    /// <example>463f29c6-2b8f-49e3-a449-6e1dde63b7b7</example>
    [Required] public Guid TierId { get; init; }

    /// <summary>If provided, updates the name.</summary>
    public string? Name { get; init; }

    /// <summary>If provided, updates the description.</summary>
    public string? Description { get; init; }

    /// <summary>Optionally enable / disable the tier.</summary>
    public bool? IsEnabled { get; init; }
}
