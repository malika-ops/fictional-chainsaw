using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Tiers.Dtos;

public record UpdateTierRequest
{
    /// <summary>Display name – must be unique.</summary>
    /// <example>Code AMU</example>
    [Required] public string Name { get; init; } = string.Empty;

    /// <summary>Description.</summary>
    /// <example>Tiers for “Code AMU” products</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>Whether the tier is enabled.</summary>
    public bool IsEnabled { get; init; } = true;
}
