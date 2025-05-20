using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;

public record PatchSupportAccountCommand : ICommand<Guid>
{
    // The ID from the route
    public Guid SupportAccountId { get; }

    // The optional fields to update
    public string? Code { get; }
    public string? Name { get; }
    public decimal? Threshold { get; }
    public decimal? Limit { get; }
    public decimal? AccountBalance { get; }
    public string? AccountingNumber { get; }
    public Guid? PartnerId { get; }
    public SupportAccountType? SupportAccountType { get; }
    public bool? IsEnabled { get; }

    public PatchSupportAccountCommand(
        Guid supportAccountId,
        string? code = null,
        string? name = null,
        decimal? threshold = null,
        decimal? limit = null,
        decimal? accountBalance = null,
        string? accountingNumber = null,
        Guid? partnerId = null,
        SupportAccountType? supportAccountType = null,
        bool? isEnabled = null)
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