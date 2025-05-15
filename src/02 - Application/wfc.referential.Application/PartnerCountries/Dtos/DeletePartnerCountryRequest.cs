using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.PartnerCountries.Dtos;

public record DeletePartnerCountryRequest
{
    /// <summary>PartnerCountry GUID (route).</summary>
    [Required] public Guid PartnerCountryId { get; init; }
}
