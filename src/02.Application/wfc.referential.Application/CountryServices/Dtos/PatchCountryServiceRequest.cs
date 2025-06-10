namespace wfc.referential.Application.CountryServices.Dtos;

public record PatchCountryServiceRequest
{
    /// <summary>Country Service association ID (from route).</summary>
    public Guid CountryServiceId { get; init; }

    /// <summary>Country ID for the association.</summary>
    public Guid? CountryId { get; init; }

    /// <summary>Service ID for the association.</summary>
    public Guid? ServiceId { get; init; }

    /// <summary>Association status (enabled/disabled).</summary>
    public bool? IsEnabled { get; init; }
}
