namespace wfc.referential.Application.Partners.Dtos;

public record GetFiltredPartnersRequest : FilterRequest
{
    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by person type.</summary>
    public string? PersonType { get; init; }

    /// <summary>Optional filter by professional tax number.</summary>
    public string? ProfessionalTaxNumber { get; init; }

    /// <summary>Optional filter by headquarters city.</summary>
    public string? HeadquartersCity { get; init; }

    /// <summary>Optional filter by tax identification number.</summary>
    public string? TaxIdentificationNumber { get; init; }

    /// <summary>Optional filter by ICE.</summary>
    public string? ICE { get; init; }

}