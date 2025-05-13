namespace wfc.referential.Application.Partners.Dtos;

public record GetAllPartnersRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by label.</summary>
    public string? Label { get; init; }

    /// <summary>Optional filter by network mode.</summary>
    public string? NetworkMode { get; init; }

    /// <summary>Optional filter by payment mode.</summary>
    public string? PaymentMode { get; init; }

    /// <summary>Optional filter by partner ID.</summary>
    public string? IdPartner { get; init; }

    /// <summary>Optional filter by support account type.</summary>
    public string? SupportAccountType { get; init; }

    /// <summary>Optional filter by identification number.</summary>
    public string? IdentificationNumber { get; init; }

    /// <summary>Optional filter by ICE.</summary>
    public string? ICE { get; init; }

    /// <summary>Optional filter by Sector ID.</summary>
    public Guid? SectorId { get; init; }

    /// <summary>Optional filter by City ID.</summary>
    public Guid? CityId { get; init; }

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}