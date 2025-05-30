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

namespace wfc.referential.Application.Agencies.Commands.UpdateAgency;

public class UpdateAgencyCommandHandler
    : ICommandHandler<UpdateAgencyCommand, Result<bool>>
{
    private readonly IAgencyRepository _agencyRepo;
    private readonly ICityRepository _cityRepo;
    private readonly ISectorRepository _sectorRepo;
    private readonly IParamTypeRepository _paramRepo;
    private readonly IPartnerRepository _partnerRepo;
    private readonly ISupportAccountRepository _supportRepo;

    public UpdateAgencyCommandHandler(
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


    public async Task<Result<bool>> Handle(UpdateAgencyCommand cmd, CancellationToken ct)
    {
        var agency = await _agencyRepo.GetByIdAsync(AgencyId.Of(cmd.AgencyId), ct);
        if (agency is null)
            throw new ResourceNotFoundException($"Agency [{cmd.AgencyId}] not found.");

        var duplicate = await _agencyRepo.GetOneByConditionAsync(a => a.Code == cmd.Code, ct);
        if (duplicate is not null && duplicate.Id != agency.Id)
            throw new AgencyCodeAlreadyExistException(cmd.Code);

        if (cmd.CityId.HasValue &&
            await _cityRepo.GetByIdAsync(CityId.Of(cmd.CityId.Value), ct) is null)
            throw new ResourceNotFoundException($"City [{cmd.CityId}] not found.");

        if (cmd.SectorId.HasValue &&
            await _sectorRepo.GetByIdAsync(SectorId.Of(cmd.SectorId.Value), ct) is null)
            throw new ResourceNotFoundException($"Sector [{cmd.SectorId}] not found.");

        if (cmd.AgencyTypeId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(cmd.AgencyTypeId.Value), ct) is null)
            throw new ResourceNotFoundException($"AgencyType [{cmd.AgencyTypeId}] not found.");

        if (cmd.TokenUsageStatusId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(cmd.TokenUsageStatusId.Value), ct) is null)
            throw new ResourceNotFoundException($"TokenUsageStatus [{cmd.TokenUsageStatusId}] not found.");

        if (cmd.FundingTypeId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(cmd.FundingTypeId.Value), ct) is null)
            throw new ResourceNotFoundException($"FundingType [{cmd.FundingTypeId}] not found.");

        if (cmd.PartnerId.HasValue &&
            await _partnerRepo.GetByIdAsync(PartnerId.Of(cmd.PartnerId.Value), ct) is null)
            throw new ResourceNotFoundException($"Partner [{cmd.PartnerId}] not found.");

        if (cmd.SupportAccountId.HasValue &&
            await _supportRepo.GetByIdAsync(SupportAccountId.Of(cmd.SupportAccountId.Value), ct) is null)
            throw new ResourceNotFoundException($"SupportAccount [{cmd.SupportAccountId}] not found.");

        agency.Update(
           code: cmd.Code,
           name: cmd.Name,
           abbreviation: cmd.Abbreviation,
           address1: cmd.Address1,
           address2: cmd.Address2,
           phone: cmd.Phone,
           fax: cmd.Fax,
           accountingSheetName: cmd.AccountingSheetName,
           accountingAccountNumber: cmd.AccountingAccountNumber,
           postalCode: cmd.PostalCode,
           latitude: cmd.Latitude,
           longitude: cmd.Longitude,
           cashTransporter: cmd.CashTransporter,
           expenseFundAccountingSheet: cmd.ExpenseFundAccountingSheet,
           expenseFundAccountNumber: cmd.ExpenseFundAccountNumber,
           madAccount: cmd.MadAccount,
           fundingThreshold: cmd.FundingThreshold,
           isEnabled: cmd.IsEnabled,
           cityId: cmd.CityId,
           sectorId: cmd.SectorId,
           agencyTypeId: cmd.AgencyTypeId,
           tokenUsageStatusId: cmd.TokenUsageStatusId,
           fundingTypeId: cmd.FundingTypeId,
           partnerId: cmd.PartnerId,
           supportAccountId: cmd.SupportAccountId);


        await _agencyRepo.SaveChangesAsync(ct);

        return Result.Success(true);

    }
}