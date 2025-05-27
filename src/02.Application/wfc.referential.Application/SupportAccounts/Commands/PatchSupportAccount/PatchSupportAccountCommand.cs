using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;

public record PatchSupportAccountCommand : ICommand<Result<bool>>
{
    public Guid SupportAccountId { get; init; }
    public string? Code { get; init; }
    public string? Description { get; init; }
    public decimal? Threshold { get; init; }
    public decimal? Limit { get; init; }
    public decimal? AccountBalance { get; init; }
    public string? AccountingNumber { get; init; }
    public bool? IsEnabled { get; init; }
}