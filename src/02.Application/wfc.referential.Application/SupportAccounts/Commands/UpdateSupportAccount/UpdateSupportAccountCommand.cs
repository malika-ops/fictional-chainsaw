using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;

public record UpdateSupportAccountCommand : ICommand<Result<bool>>
{
    public Guid SupportAccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public decimal Limit { get; set; }
    public decimal AccountBalance { get; set; }
    public string AccountingNumber { get; set; } = string.Empty;
    public SupportAccountTypeEnum SupportAccountType { get; set; }
    public bool IsEnabled { get; set; } = true;
}