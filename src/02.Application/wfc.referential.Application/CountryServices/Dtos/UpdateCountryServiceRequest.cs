namespace wfc.referential.Application.CountryServices.Dtos;

public record UpdateCountryServiceRequest
{
    /// <summary>Country ID for the association.</summary>
    public Guid CountryId { get; init; }

    /// <summary>Service ID for the association.</summary>
    public Guid ServiceId { get; init; }

    /// <summary>Association status (enabled/disabled).</summary>
    public bool IsEnabled { get; init; } = true;
}