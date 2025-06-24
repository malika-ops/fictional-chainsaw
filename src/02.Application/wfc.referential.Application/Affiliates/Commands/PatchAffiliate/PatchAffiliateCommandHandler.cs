using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.AffiliateAggregate.Exceptions;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Affiliates.Commands.PatchAffiliate;

public class PatchAffiliateCommandHandler : ICommandHandler<PatchAffiliateCommand, Result<bool>>
{
    private readonly IAffiliateRepository _repo;
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly ICountryRepository _countryRepository;

    public PatchAffiliateCommandHandler(
        IAffiliateRepository repo,
        IParamTypeRepository paramTypeRepository,
        ICountryRepository countryRepository)
    {
        _repo = repo;
        _paramTypeRepository = paramTypeRepository;
        _countryRepository = countryRepository;
    }

    public async Task<Result<bool>> Handle(PatchAffiliateCommand cmd, CancellationToken ct)
    {
        var affiliate = await _repo.GetByIdAsync(AffiliateId.Of(cmd.AffiliateId), ct);
        if (affiliate is null)
            throw new ResourceNotFoundException($"Affiliate [{cmd.AffiliateId}] not found.");

        // Check if code is unique (if being updated)
        if (cmd.Code != null && cmd.Code != affiliate.Code)
        {
            var existingByCode = await _repo.GetByConditionAsync(a => a.Code == cmd.Code, ct);
            if (existingByCode.Any())
                throw new AffiliateCodeAlreadyExistException(cmd.Code);
        }
        CountryId? countryId = null;

        // Validate Country exists if provided
        if (cmd.CountryId.HasValue)
        {
            countryId = CountryId.Of(cmd.CountryId.Value);
            var country = await _countryRepository.GetByIdAsync(countryId, ct);
            if (country is null)
                throw new ResourceNotFoundException($"Country with ID {cmd.CountryId.Value} not found");
        }

        // Validate AffiliateType exists if provided
        if (cmd.AffiliateTypeId.HasValue)
        {
            var affiliateType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(cmd.AffiliateTypeId.Value), ct);
            if (affiliateType is null)
                throw new ResourceNotFoundException($"Affiliate Type with ID {cmd.AffiliateTypeId.Value} not found");
        }

        affiliate.Patch(
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
            cmd.IsEnabled);

        // Set relationships after validation (only if provided)
        if (cmd.AffiliateTypeId.HasValue)
            affiliate.SetAffiliateType(ParamTypeId.Of(cmd.AffiliateTypeId.Value));

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}