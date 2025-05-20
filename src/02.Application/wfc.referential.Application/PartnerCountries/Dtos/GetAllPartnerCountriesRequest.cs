namespace wfc.referential.Application.PartnerCountries.Dtos;

public record GetAllPartnerCountriesRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by PartnerId.</summary>
    public Guid? PartnerId { get; init; }

    /// <summary>Filter by CountryId.</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Status filter.</summary>
    public bool? IsEnabled { get; init; } = true;
}
