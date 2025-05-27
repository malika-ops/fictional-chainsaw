using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Banks.Commands.UpdateBank;

public record UpdateBankCommand : ICommand<Result<bool>>
{
    public Guid BankId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}