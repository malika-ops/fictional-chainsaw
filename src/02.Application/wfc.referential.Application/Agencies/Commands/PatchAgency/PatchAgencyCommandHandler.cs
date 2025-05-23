using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyAggregate.Exceptions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Agencies.Commands.PatchAgency;

public class PatchAgencyCommandHandler : ICommandHandler<PatchAgencyCommand, Result<bool>>
{
    private readonly IAgencyRepository _repo;
    private readonly ICityRepository _cityRepo;
    private readonly ISectorRepository _sectorRepo;
    private readonly IParamTypeRepository _paramTypeRepo;

    public PatchAgencyCommandHandler(IAgencyRepository repo, ICityRepository cityRepo)
    {
        _repo = repo;
        _cityRepo = cityRepo;
    }

    public async Task<Result<bool>> Handle(PatchAgencyCommand cmd, CancellationToken ct)
    {
        var agency = await _repo.GetByIdAsync(AgencyId.Of(cmd.AgencyId), ct);
        if (agency is null)
            throw new ResourceNotFoundException($"Agency [{cmd.AgencyId}] not found.");

        // duplicate Code check
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var dup = await _repo.GetOneByConditionAsync(a => a.Code == cmd.Code, ct);
            if (dup is not null && dup.Id != agency.Id)
                throw new AgencyCodeAlreadyExistException(cmd.Code);
        }
        // CityId check if not null
        if (cmd.CityId.HasValue)
        {
            var city = await _cityRepo.GetByIdAsync(CityId.Of(cmd.CityId.Value), ct);
            if (city == null)
                throw new ResourceNotFoundException($"City with Id {cmd.CityId.Value} not found");
        }
        // SectorId check if not null
        if (cmd.SectorId.HasValue)
        {
            var sector = await _sectorRepo.GetByIdAsync(SectorId.Of(cmd.SectorId.Value), ct);
            if (sector == null)
                throw new ResourceNotFoundException($"sector with Id {cmd.SectorId.Value} not found");
        }
        // AgencyTypeId check if not null
        if (cmd.AgencyTypeId.HasValue)
        {
            var agencyType = await _paramTypeRepo.GetByIdAsync(ParamTypeId.Of(cmd.AgencyTypeId.Value), ct);
            if (agencyType == null)
                throw new ResourceNotFoundException($"agencyType with Id {cmd.AgencyTypeId.Value} not found");
        }

        agency.Patch(
                cmd.Code,
                cmd.Name,
                cmd.Abbreviation,
                cmd.Address1,
                cmd.Address2,
                cmd.Phone,
                cmd.Fax,
                cmd.AccountingSheetName,
                cmd.AccountingAccountNumber,
                cmd.MoneyGramReferenceNumber,
                cmd.MoneyGramPassword,
                cmd.PostalCode,
                cmd.PermissionOfficeChange,
                cmd.Latitude,
                cmd.Longitude,
                cmd.CityId ,
                cmd.SectorId ,
                cmd.AgencyTypeId,
                cmd.SupportAccountId,
                cmd.PartnerId,
                cmd.IsEnabled);

        _repo.Update(agency);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}