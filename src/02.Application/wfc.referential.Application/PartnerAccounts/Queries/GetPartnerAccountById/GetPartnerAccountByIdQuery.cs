using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.PartnerAccounts.Dtos;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetPartnerAccountById;

public record GetPartnerAccountByIdQuery : IQuery<PartnerAccountResponse>
{
    public Guid PartnerAccountId { get; init; }
} 