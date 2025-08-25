using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.AffiliateAggregate.Exceptions;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Affiliates.Commands.UpdateAffiliate;

public class UpdateAffiliateCommandHandler : ICommandHandler<UpdateAffiliateCommand, Result<bool>>
{
    private readonly IAffiliateRepository _repo;
    private readonly ICountryRepository _countryRepository;

    public UpdateAffiliateCommandHandler(
        IAffiliateRepository repo,
        ICountryRepository countryRepository)
    {
        _repo = repo;
        _countryRepository = countryRepository;
    }

    public async Task<Result<bool>> Handle(UpdateAffiliateCommand cmd, CancellationToken ct)
    {
        var affiliate = await _repo.GetByIdAsync(AffiliateId.Of(cmd.AffiliateId), ct);
        if (affiliate is null)
            throw new ResourceNotFoundException($"Affiliate [{cmd.AffiliateId}] not found.");

        // Check if code is unique (if changed)
        if (cmd.Code != affiliate.Code)
        {
            var existingByCode = await _repo.GetByConditionAsync(a => a.Code == cmd.Code, ct);
            if (existingByCode.Any())
                throw new AffiliateCodeAlreadyExistException(cmd.Code);
        }

        var countryId = CountryId.Of(cmd.CountryId);

        // Validate Country exists
        var country = await _countryRepository.GetByIdAsync(countryId, ct);
        if (country is null)
            throw new ResourceNotFoundException($"Country with ID {cmd.CountryId} not found");

        affiliate.Update(
            cmd.Code,
            cmd.Name,
            cmd.Abbreviation,
            cmd.OpeningDate,
            cmd.CancellationDay,
            cmd.Logo,
            cmd.ThresholdBilling,
            cmd.AccountingDocumentNumber,
            cmd.AccountingAccountNumber,
            cmd.StampDutyMention,
           countryId,
           cmd.AffiliateType,
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}