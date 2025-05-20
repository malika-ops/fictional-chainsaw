using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate.Exceptions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Agencies.Commands.PatchAgency;

public class PatchAgencyCommandHandler : ICommandHandler<PatchAgencyCommand, Result<Guid>>
{
    private readonly IAgencyRepository _repo;

    public PatchAgencyCommandHandler(IAgencyRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(PatchAgencyCommand cmd, CancellationToken ct)
    {
        var agency = await _repo.GetByIdAsync(cmd.AgencyId, ct);
        if (agency is null)
            throw new BusinessException($"Agency [{cmd.AgencyId}] not found.");

        // duplicate Code check
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var dup = await _repo.GetByCodeAsync(cmd.Code, ct);
            if (dup is not null && dup.Id != agency.Id)
                throw new AgencyCodeAlreadyExistException(cmd.Code);
        }

        // Map only non-nulls to aggregates
        cmd.Adapt(agency);

        // handle value-object conversions if IDs provided
        if (cmd.CityId.HasValue || cmd.SectorId.HasValue || cmd.AgencyTypeId.HasValue)
        {
            agency.Update(
                agency.Code,
                agency.Name,
                agency.Abbreviation,
                agency.Address1,
                agency.Address2,
                agency.Phone,
                agency.Fax,
                agency.AccountingSheetName,
                agency.AccountingAccountNumber,
                agency.MoneyGramReferenceNumber,
                agency.MoneyGramPassword,
                agency.PostalCode,
                agency.PermissionOfficeChange,
                agency.Latitude,
                agency.Longitude,
                cmd.CityId is null ? agency.CityId : new CityId(cmd.CityId.Value),
                cmd.SectorId is null ? agency.SectorId : new SectorId(cmd.SectorId.Value),
                cmd.AgencyTypeId is null ? agency.AgencyTypeId : new ParamTypeId(cmd.AgencyTypeId.Value),
                agency.SupportAccountId,
                agency.PartnerId,
                agency.IsEnabled);
        }

        agency.Patch();                 // raise event
        await _repo.UpdateAsync(agency, ct);

        return Result.Success(agency.Id!.Value);
    }
}