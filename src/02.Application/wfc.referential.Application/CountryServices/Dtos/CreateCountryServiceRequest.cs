namespace wfc.referential.Application.CountryServices.Dtos;

public record CreateCountryServiceRequest
{
    /// <summary>Country ID for the association.</summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid CountryId { get; init; }

    /// <summary>Service ID for the association.</summary>
    /// <example>456e7890-e89b-12d3-a456-426614174001</example>
    public Guid ServiceId { get; init; }
}
