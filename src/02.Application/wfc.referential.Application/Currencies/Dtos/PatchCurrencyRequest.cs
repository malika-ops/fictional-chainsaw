using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Currencies.Dtos;

public record PatchCurrencyRequest
{
    /// <summary>
    /// The ID of the Currency to patch.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid CurrencyId { get; init; }

    /// <summary> 
    /// If provided, updates the code. If omitted, code remains unchanged. 
    /// </summary>
    public string? Code { get; init; }

    /// <summary> 
    /// If provided, updates the Arabic code. If omitted, codeAR remains unchanged. 
    /// </summary>
    public string? CodeAR { get; init; }

    /// <summary> 
    /// If provided, updates the English code. If omitted, codeEN remains unchanged. 
    /// </summary>
    public string? CodeEN { get; init; }

    /// <summary> 
    /// If provided, updates the name. If omitted, name remains unchanged. 
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// If provided, updates the 3-digit number. If omitted, codeiso remains unchanged.
    /// Must be between 0 and 999.
    /// </summary>
    public int? CodeIso { get; init; }

    /// <summary> 
    /// If provided, updates the enabled status. If omitted, status remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }
}