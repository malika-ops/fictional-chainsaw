using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.PartnerCountries.Dtos;

public record UpdatePartnerCountryRequest
{
    [Required] public Guid PartnerCountryId { get; init; }
    [Required] public Guid PartnerId { get; init; }
    [Required] public Guid CountryId { get; init; }

    /// <summary>Status flag (default = true).</summary>
    public bool IsEnabled { get; init; } = true;
}
