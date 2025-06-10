namespace wfc.referential.Application.CountryServices.Dtos;

public record GetAllCountryServicesRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by Country ID.</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Filter by Service ID.</summary>
    public Guid? ServiceId { get; init; }

    /// <summary>Status filter (Enabled/Disabled).</summary>
    public bool? IsEnabled { get; init; } = true;
}