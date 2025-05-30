using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyAggregate.Exceptions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Agencies.Commands.PatchAgency;

public class PatchAgencyCommandHandler : ICommandHandler<PatchAgencyCommand, Result<bool>>
{
    private readonly IAgencyRepository _agencyRepo;
    private readonly ICityRepository _cityRepo;
    private readonly ISectorRepository _sectorRepo;
    private readonly IParamTypeRepository _paramRepo;
    private readonly IPartnerRepository _partnerRepo;
    private readonly ISupportAccountRepository _supportRepo;

    public PatchAgencyCommandHandler(
        IAgencyRepository agencyRepo,
        ICityRepository cityRepo,
        ISectorRepository sectorRepo,
        IParamTypeRepository paramRepo,
        IPartnerRepository partnerRepo,
        ISupportAccountRepository supportRepo)
    {
        _agencyRepo = agencyRepo;
        _cityRepo = cityRepo;
        _sectorRepo = sectorRepo;
        _paramRepo = paramRepo;
        _partnerRepo = partnerRepo;
        _supportRepo = supportRepo;
    }

    public async Task<Result<bool>> Handle(PatchAgencyCommand c, CancellationToken ct)
    {
        var agency = await _agencyRepo.GetByIdAsync(AgencyId.Of(c.AgencyId), ct);
        if (agency is null)
            throw new ResourceNotFoundException($"Agency [{c.AgencyId}] not found.");

        if (!string.IsNullOrWhiteSpace(c.Code))
        {
            var dup = await _agencyRepo.GetOneByConditionAsync(a => a.Code == c.Code, ct);
            if (dup is not null && dup.Id != agency.Id)
                throw new AgencyCodeAlreadyExistException(c.Code);
        }

        if (c.CityId.HasValue &&
            await _cityRepo.GetByIdAsync(CityId.Of(c.CityId.Value), ct) is null)
            throw new ResourceNotFoundException($"City [{c.CityId}] not found.");

        if (c.SectorId.HasValue &&
            await _sectorRepo.GetByIdAsync(SectorId.Of(c.SectorId.Value), ct) is null)
            throw new ResourceNotFoundException($"Sector [{c.SectorId}] not found.");

        if (c.AgencyTypeId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(c.AgencyTypeId.Value), ct) is null)
            throw new ResourceNotFoundException($"AgencyType [{c.AgencyTypeId}] not found.");

        if (c.TokenUsageStatusId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(c.TokenUsageStatusId.Value), ct) is null)
            throw new ResourceNotFoundException($"TokenUsageStatus [{c.TokenUsageStatusId}] not found.");

        if (c.FundingTypeId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(c.FundingTypeId.Value), ct) is null)
            throw new ResourceNotFoundException($"FundingType [{c.FundingTypeId}] not found.");

        if (c.PartnerId.HasValue &&
            await _partnerRepo.GetByIdAsync(PartnerId.Of(c.PartnerId.Value), ct) is null)
            throw new ResourceNotFoundException($"Partner [{c.PartnerId}] not found.");

        if (c.SupportAccountId.HasValue &&
            await _supportRepo.GetByIdAsync(SupportAccountId.Of(c.SupportAccountId.Value), ct) is null)
            throw new ResourceNotFoundException($"SupportAccount [{c.SupportAccountId}] not found.");

        agency.Patch(
            code: c.Code,
            name: c.Name,
            abbreviation: c.Abbreviation,
            address1: c.Address1,
            address2: c.Address2,
            phone: c.Phone,
            fax: c.Fax,
            accountingSheetName: c.AccountingSheetName,
            accountingAccountNumber: c.AccountingAccountNumber,
            postalCode: c.PostalCode,
            latitude: c.Latitude,
            longitude: c.Longitude,
            cashTransporter: c.CashTransporter,
            expenseFundAccountingSheet: c.ExpenseFundAccountingSheet,
            expenseFundAccountNumber: c.ExpenseFundAccountNumber,
            madAccount: c.MadAccount,
            fundingThreshold: c.FundingThreshold,
            isEnabled: c.IsEnabled,
            cityId: c.CityId,
            sectorId: c.SectorId,
            agencyTypeId: c.AgencyTypeId,
            tokenUsageStatusId: c.TokenUsageStatusId,
            fundingTypeId: c.FundingTypeId,
            partnerId: c.PartnerId,
            supportAccountId: c.SupportAccountId);

        _agencyRepo.Update(agency);
        await _agencyRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}