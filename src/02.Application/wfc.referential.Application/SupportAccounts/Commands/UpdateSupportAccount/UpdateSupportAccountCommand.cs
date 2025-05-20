using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;

public record UpdateSupportAccountCommand : ICommand<Guid>
{
    public Guid SupportAccountId { get; }
    public string Code { get; }
    public string Name { get; }
    public decimal Threshold { get; }
    public decimal Limit { get; }
    public decimal AccountBalance { get; }
    public string AccountingNumber { get; }
    public Guid PartnerId { get; }
    public SupportAccountType SupportAccountType { get; }
    public bool IsEnabled { get; }

    public UpdateSupportAccountCommand(
        Guid supportAccountId,
        string code,
        string name,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        Guid partnerId,
        SupportAccountType supportAccountType,
        bool isEnabled)
    {
        SupportAccountId = supportAccountId;
        Code = code;
        Name = name;
        Threshold = threshold;
        Limit = limit;
        AccountBalance = accountBalance;
        AccountingNumber = accountingNumber;
        PartnerId = partnerId;
        SupportAccountType = supportAccountType;
        IsEnabled = isEnabled;
    }
}