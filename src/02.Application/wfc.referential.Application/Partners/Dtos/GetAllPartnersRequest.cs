namespace wfc.referential.Application.Partners.Dtos;

public record GetAllPartnersRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

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

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}