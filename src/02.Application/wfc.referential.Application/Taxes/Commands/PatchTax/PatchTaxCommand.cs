using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.PatchTax;

/// <summary>
/// Command to update a tax's details.
/// </summary>
public record PatchTaxCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    /// <summary>
    /// The ID of the tax to be updated.
    /// </summary>
    public Guid TaxId { get; init; }

    /// <summary>Optional tax code to update.</summary>
    public string? Code { get; init; }

    /// <summary>Optional English tax code to update.</summary>
    public string? CodeEn { get; init; }

    /// <summary>Optional Arabic tax code to update.</summary>
    public string? CodeAr { get; init; }

    /// <summary>Optional description of the tax to update.</summary>
    public string? Description { get; init; }

    /// <summary>Optional tax FixedAmount to update.</summary>
    public double? FixedAmount { get; init; }

    /// <summary>Optional tax value to update.</summary>
    public double? Value { get; init; }

    /// <summary>Optional tax status to update.</summary>
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(Tax)}_{TaxId}";

    public int CacheExpiration => 5;
}

