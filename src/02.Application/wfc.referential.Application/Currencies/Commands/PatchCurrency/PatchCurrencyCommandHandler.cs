using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exceptions;

namespace wfc.referential.Application.Currencies.Commands.PatchCurrency;

public class PatchCurrencyCommandHandler : ICommandHandler<PatchCurrencyCommand, Result<bool>>
{
    private readonly ICurrencyRepository _repo;

    public PatchCurrencyCommandHandler(ICurrencyRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(PatchCurrencyCommand cmd, CancellationToken ct)
    {
        var currency = await _repo.GetByIdAsync(CurrencyId.Of(cmd.CurrencyId), ct);
        if (currency is null)
            throw new ResourceNotFoundException($"Currency [{cmd.CurrencyId}] not found.");

        // duplicate Code check
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var dup = await _repo.GetOneByConditionAsync(c => c.Code == cmd.Code, ct);
            if (dup is not null && dup.Id != currency.Id)
                throw new CurrencyCodeAlreadyExistException(cmd.Code);
        }

        // duplicate CodeIso check
        if (cmd.CodeIso.HasValue)
        {
            var dup = await _repo.GetOneByConditionAsync(c => c.CodeIso == cmd.CodeIso.Value, ct);
            if (dup is not null && dup.Id != currency.Id)
                throw new CurrencyCodeIsoAlreadyExistException(cmd.CodeIso.Value);
        }

        currency.Patch(
            cmd.Code,
            cmd.CodeAR,
            cmd.CodeEN,
            cmd.Name,
            cmd.CodeIso,
            cmd.IsEnabled);

        _repo.Update(currency);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
