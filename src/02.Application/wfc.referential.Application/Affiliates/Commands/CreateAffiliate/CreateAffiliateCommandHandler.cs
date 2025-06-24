using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.AffiliateAggregate.Exceptions;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Affiliates.Commands.CreateAffiliate;

public class CreateAffiliateCommandHandler : ICommandHandler<CreateAffiliateCommand, Result<Guid>>
{
    private readonly IAffiliateRepository _repo;
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly ICountryRepository _countryRepository;

    public CreateAffiliateCommandHandler(
        IAffiliateRepository repo,
        IParamTypeRepository paramTypeRepository,
        ICountryRepository countryRepository)
    {
        _repo = repo;
        _paramTypeRepository = paramTypeRepository;
        _countryRepository = countryRepository;
    }

    public async Task<Result<Guid>> Handle(CreateAffiliateCommand cmd, CancellationToken ct)
    {
        // Check if the code already exists
        var existingByCode = await _repo.GetByConditionAsync(a => a.Code == cmd.Code, ct);
        if (existingByCode.Any())
            throw new AffiliateCodeAlreadyExistException(cmd.Code);
        var countryId = CountryId.Of(cmd.CountryId);
        // Validate Country exists
        var country = await _countryRepository.GetByIdAsync(countryId, ct);
        if (country is null)
            throw new ResourceNotFoundException($"Country with ID {cmd.CountryId} not found");

        // Validate AffiliateType exists 
        var affiliateType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(cmd.AffiliateTypeId), ct);
        if (affiliateType is null)
            throw new ResourceNotFoundException($"Affiliate Type with ID {cmd.AffiliateTypeId} not found");

        var id = AffiliateId.Of(Guid.NewGuid());
        var affiliate = Affiliate.Create(
            id,
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
           countryId);

        // Set relationships after validation
        affiliate.SetAffiliateType(ParamTypeId.Of(cmd.AffiliateTypeId));

        await _repo.AddAsync(affiliate, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(affiliate.Id.Value);
    }
}