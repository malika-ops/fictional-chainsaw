using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Queries.GetAllPartners;

public record GetAllPartnersQuery : IQuery<PagedResult<PartnerResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Code { get; init; }
    public string? Label { get; init; }
    public string? NetworkMode { get; init; }
    public string? PaymentMode { get; init; }
    public string? IdPartner { get; init; }
    public string? SupportAccountType { get; init; }
    public string? IdentificationNumber { get; init; }
    public string? ICE { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? CityId { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllPartnersQuery(
        int pageNumber,
        int pageSize,
        string? code = null,
        string? label = null,
        string? networkMode = null,
        string? paymentMode = null,
        string? idPartner = null,
        string? supportAccountType = null,
        string? identificationNumber = null,
        string? ice = null,
        Guid? sectorId = null,
        Guid? cityId = null,
        bool? isEnabled = null)
    {
        Code = code;
        Label = label;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        IdPartner = idPartner;
        SupportAccountType = supportAccountType;
        IdentificationNumber = identificationNumber;
        ICE = ice;
        SectorId = sectorId;
        CityId = cityId;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}