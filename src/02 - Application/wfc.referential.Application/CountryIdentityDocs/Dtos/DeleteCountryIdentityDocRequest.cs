using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record DeleteCountryIdentityDocRequest
{
    /// <summary>
    /// The ID of the Country Identity Document association to delete.
    /// </summary>

    public Guid CountryIdentityDocId { get; init; }
}