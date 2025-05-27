using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;

public record CreateSupportAccountCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Threshold { get; init; }
    public decimal Limit { get; init; }
    public decimal AccountBalance { get; init; }
    public string AccountingNumber { get; init; } = string.Empty;
}