using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.CreateBank;

public class CreateBankCommandHandler : ICommandHandler<CreateBankCommand, Result<Guid>>
{
    private readonly IBankRepository _bankRepository;

    public CreateBankCommandHandler(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<Result<Guid>> Handle(CreateBankCommand request, CancellationToken cancellationToken)
    {
        // Check if the code already exists
        var existingCode = await _bankRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode is not null)
            throw new BankCodeAlreadyExistException(request.Code);

        var id = BankId.Of(Guid.NewGuid());
        var bank = Bank.Create(id, request.Code, request.Name, request.Abbreviation);

        await _bankRepository.AddBankAsync(bank, cancellationToken);

        return Result.Success(bank.Id.Value);
    }
}