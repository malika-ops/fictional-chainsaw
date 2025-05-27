using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Banks.Commands.DeleteBank;

public record DeleteBankCommand : ICommand<Result<bool>>
{
    public Guid BankId { get; }
    public DeleteBankCommand(Guid bankId) => BankId = bankId;
}