using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public class DeleteCurrencyCommandHandler : ICommandHandler<DeleteCurrencyCommand, Result<bool>>
{
    private readonly ICurrencyRepository _repo;

    public DeleteCurrencyCommandHandler(ICurrencyRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteCurrencyCommand cmd, CancellationToken ct)
    {
        var currency = await _repo.GetByIdAsync(CurrencyId.Of(cmd.CurrencyId), ct);
        if (currency is null)
            throw new BusinessException($"Currency [{cmd.CurrencyId}] not found.");

        currency.Disable();
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}