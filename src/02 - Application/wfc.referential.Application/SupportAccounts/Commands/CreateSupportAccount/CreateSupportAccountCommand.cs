using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;

public record CreateSupportAccountCommand : ICommand<Result<Guid>>
{
    public string Code { get; }
    public string Name { get; }
    public decimal Threshold { get; }
    public decimal Limit { get; }
    public decimal AccountBalance { get; }
    public string AccountingNumber { get; }
    public Guid PartnerId { get; }
    public SupportAccountType SupportAccountType { get; }

    public CreateSupportAccountCommand(
        string code,
        string name,
        decimal threshold,
        decimal limit,
        decimal accountBalance,
        string accountingNumber,
        Guid partnerId,
        SupportAccountType supportAccountType)
    {
        Code = code;
        Name = name;
        Threshold = threshold;
        Limit = limit;
        AccountBalance = accountBalance;
        AccountingNumber = accountingNumber;
        PartnerId = partnerId;
        SupportAccountType = supportAccountType;
    }
}