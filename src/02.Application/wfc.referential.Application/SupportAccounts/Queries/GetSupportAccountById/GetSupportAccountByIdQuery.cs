using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.SupportAccounts.Dtos;

namespace wfc.referential.Application.SupportAccounts.Queries.GetSupportAccountById;

public record GetSupportAccountByIdQuery : IQuery<GetSupportAccountsResponse>
{
    public Guid SupportAccountId { get; init; }
} 