using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Tiers.Dtos;

public record CreateTierRequest
{
    /// <summary>Display name (must be unique).</summary>
    /// <example>Code AMU</example>
    [Required] public string Name { get; init; } = string.Empty;

    /// <summary>Human readable description.</summary>
    /// <example>Tiers for “Code AMU” products</example>
    public string Description { get; init; } = string.Empty;
}
