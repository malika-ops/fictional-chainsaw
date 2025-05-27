using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exceptions;

namespace wfc.referential.Application.Currencies.Commands.UpdateCurrency;

public class UpdateCurrencyCommandHandler
    : ICommandHandler<UpdateCurrencyCommand, Result<bool>>
{
    private readonly ICurrencyRepository _repo;

    public UpdateCurrencyCommandHandler(ICurrencyRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateCurrencyCommand cmd, CancellationToken ct)
    {
        var currency = await _repo.GetByIdAsync(CurrencyId.Of(cmd.CurrencyId), ct);
        if (currency is null)
            throw new BusinessException($"Currency [{cmd.CurrencyId}] not found.");

        // uniqueness on Code
        var duplicateCode = await _repo.GetOneByConditionAsync(c => c.Code == cmd.Code, ct);
        if (duplicateCode is not null && duplicateCode.Id != currency.Id)
            throw new CurrencyCodeAlreadyExistException(cmd.Code);

        // uniqueness on CodeIso
        var duplicateCodeIso = await _repo.GetOneByConditionAsync(c => c.CodeIso == cmd.CodeIso, ct);
        if (duplicateCodeIso is not null && duplicateCodeIso.Id != currency.Id)
            throw new CurrencyCodeIsoAlreadyExistException(cmd.CodeIso);

        currency.Update(
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