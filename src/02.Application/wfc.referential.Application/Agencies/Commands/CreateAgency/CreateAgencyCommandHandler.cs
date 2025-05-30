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

namespace wfc.referential.Application.Agencies.Commands.CreateAgency;

public class CreateAgencyCommandHandler : ICommandHandler<CreateAgencyCommand, Result<Guid>>
{

    private readonly IAgencyRepository _agencyRepo;
    private readonly ICityRepository _cityRepo;
    private readonly ISectorRepository _sectorRepo;
    private readonly IParamTypeRepository _paramRepo;
    private readonly IPartnerRepository _partnerRepo;
    private readonly ISupportAccountRepository _supportRepo;

    public CreateAgencyCommandHandler(IAgencyRepository agencyRepo,
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

    public async Task<Result<Guid>> Handle(CreateAgencyCommand command, CancellationToken ct)
    {
        string agencyCode = string.IsNullOrEmpty(command.Code)
            ? await GenerateUniqueCodeAsync(ct)
            : command.Code;

        var existingAgency = await _agencyRepo.GetOneByConditionAsync(a => a.Code == agencyCode, ct);

        /* Duplicate-code check. */
        if (existingAgency is not null)
            throw new AgencyCodeAlreadyExistException(agencyCode);

        // check existence of related entities
        if (command.CityId.HasValue &&
            await _cityRepo.GetByIdAsync(CityId.Of(command.CityId.Value), ct) is null)
            throw new ResourceNotFoundException($"City [{command.CityId}] not found.");

        if (command.SectorId.HasValue &&
            await _sectorRepo.GetByIdAsync(SectorId.Of(command.SectorId.Value), ct) is null)
            throw new ResourceNotFoundException($"Sector [{command.SectorId}] not found.");

        if (command.AgencyTypeId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(command.AgencyTypeId.Value), ct) is null)
            throw new ResourceNotFoundException($"AgencyType [{command.AgencyTypeId}] not found.");

        if (command.TokenUsageStatusId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(command.TokenUsageStatusId.Value), ct) is null)
            throw new ResourceNotFoundException($"TokenUsageStatus [{command.TokenUsageStatusId}] not found.");

        if (command.FundingTypeId.HasValue &&
            await _paramRepo.GetByIdAsync(ParamTypeId.Of(command.FundingTypeId.Value), ct) is null)
            throw new ResourceNotFoundException($"FundingType [{command.FundingTypeId}] not found.");

        if (command.PartnerId.HasValue &&
            await _partnerRepo.GetByIdAsync(PartnerId.Of(command.PartnerId.Value), ct) is null)
            throw new ResourceNotFoundException($"Partner [{command.PartnerId}] not found.");

        if (command.SupportAccountId.HasValue &&
            await _supportRepo.GetByIdAsync(SupportAccountId.Of(command.SupportAccountId.Value), ct) is null)
            throw new ResourceNotFoundException($"SupportAccount [{command.SupportAccountId}] not found.");

        var agencyId = AgencyId.Of(Guid.NewGuid());

        var agency = Agency.Create(
            id: agencyId,
            code: agencyCode,
            name: command.Name,
            abbreviation: command.Abbreviation,
            address1: command.Address1,
            address2: command.Address2,
            phone: command.Phone,
            fax: command.Fax,
            accountingSheetName: command.AccountingSheetName,
            accountingAccountNumber: command.AccountingAccountNumber,
            postalCode: command.PostalCode,
            latitude: command.Latitude,
            longitude: command.Longitude,
            cashTransporter: command.CashTransporter,
            expenseFundAccountingSheet: command.ExpenseFundAccountingSheet,
            expenseFundAccountNumber: command.ExpenseFundAccountNumber,
            madAccount: command.MadAccount,
            fundingThreshold: command.FundingThreshold,
            cityId: command.CityId.HasValue ? CityId.Of(command.CityId.Value) : null,
            sectorId: command.SectorId.HasValue ? SectorId.Of(command.SectorId.Value) : null,
            agencyTypeId: command.AgencyTypeId.HasValue ? ParamTypeId.Of(command.AgencyTypeId.Value) : null,
            tokenUsageStatusId: command.TokenUsageStatusId.HasValue ? ParamTypeId.Of(command.TokenUsageStatusId.Value) : null,
            fundingTypeId: command.FundingTypeId.HasValue ? ParamTypeId.Of(command.FundingTypeId.Value) : null,
            partnerId: command.PartnerId.HasValue ? PartnerId.Of(command.PartnerId.Value) : null,
            supportAccountId: command.SupportAccountId.HasValue ? SupportAccountId.Of(command.SupportAccountId.Value) : null
        );

        await _agencyRepo.AddAsync(agency, ct);
        await _agencyRepo.SaveChangesAsync(ct);

        return Result.Success(agency.Id!.Value);
    }

    private const int MaxCodeAttempts = 100;

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
        {
            var code = Agency.GenerateAgencyCode();

            if (await _agencyRepo.GetOneByConditionAsync(a => a.Code == code, ct) is null)
                return code;                        // unique!

            // else try again until find a unique code or max attempts reached
        }

        throw new TechnicalException(
            $"Unable to generate a unique 6-digit agency code after {MaxCodeAttempts} attempts.");
    }
}
