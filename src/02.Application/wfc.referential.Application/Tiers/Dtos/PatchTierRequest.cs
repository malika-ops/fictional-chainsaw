using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Tiers.Dtos;

public record PatchTierRequest
{
    /// <summary>If provided, updates the name.</summary>
    public string? Name { get; init; }

    /// <summary>If provided, updates the description.</summary>
    public string? Description { get; init; }

    /// <summary>Optionally enable / disable the tier.</summary>
    public bool? IsEnabled { get; init; }
}
