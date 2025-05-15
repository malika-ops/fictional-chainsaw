using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.PartnerCountries.Dtos;

public record CreatePartnerCountryRequest
{
    /// <summary>Reference to an existing Partner.</summary>
    [Required] public Guid PartnerId { get; init; }

    /// <summary>Reference to an existing Country.</summary>
    [Required] public Guid CountryId { get; init; }

}
