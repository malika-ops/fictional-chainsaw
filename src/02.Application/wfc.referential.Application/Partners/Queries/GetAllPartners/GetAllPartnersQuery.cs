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
    public string? Type { get; init; }
    public string? NetworkMode { get; init; }
    public string? PaymentMode { get; init; }
    public Guid? IdParent { get; init; }
    public string? SupportAccountType { get; init; }
    public string? TaxIdentificationNumber { get; init; }
    public string? ICE { get; init; }
    public Guid? CommissionAccountId { get; init; }
    public Guid? ActivityAccountId { get; init; }
    public Guid? SupportAccountId { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllPartnersQuery(
        int pageNumber,
        int pageSize,
        string? code = null,
        string? label = null,
        string? type = null,
        string? networkMode = null,
        string? paymentMode = null,
        Guid? idParent = null,
        string? supportAccountType = null,
        string? taxIdentificationNumber = null,
        string? ice = null,
        Guid? commissionAccountId = null,
        Guid? activityAccountId = null,
        Guid? supportAccountId = null,
        bool? isEnabled = null)
    {
        Code = code;
        Label = label;
        Type = type;
        NetworkMode = networkMode;
        PaymentMode = paymentMode;
        IdParent = idParent;
        SupportAccountType = supportAccountType;
        TaxIdentificationNumber = taxIdentificationNumber;
        ICE = ice;
        CommissionAccountId = commissionAccountId;
        ActivityAccountId = activityAccountId;
        SupportAccountId = supportAccountId;
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsEnabled = isEnabled;
    }
}