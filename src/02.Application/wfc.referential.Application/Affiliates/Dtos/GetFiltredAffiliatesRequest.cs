namespace wfc.referential.Application.Affiliates.Dtos;

public record GetFiltredAffiliatesRequest : FilterRequest
{
    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by opening date.</summary>
    public DateTime? OpeningDate { get; init; }

    /// <summary>Optional filter by cancellation date.</summary>
    public string? CancellationDay { get; init; }

    /// <summary>Optional filter by country ID.</summary>
    public Guid? CountryId { get; init; }

}