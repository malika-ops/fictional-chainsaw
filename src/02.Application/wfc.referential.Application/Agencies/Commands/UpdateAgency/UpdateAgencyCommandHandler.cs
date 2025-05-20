using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate.Exceptions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Agencies.Commands.UpdateAgency;

public class UpdateAgencyCommandHandler
    : ICommandHandler<UpdateAgencyCommand, Result<Guid>>
{
    private readonly IAgencyRepository _repo;

    public UpdateAgencyCommandHandler(IAgencyRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(UpdateAgencyCommand cmd, CancellationToken ct)
    {
        var agency = await _repo.GetByIdAsync(cmd.AgencyId, ct);
        if (agency is null)
            throw new BusinessException($"Agency [{cmd.AgencyId}] not found.");

        // uniqueness on Code
        var duplicate = await _repo.GetByCodeAsync(cmd.Code, ct);
        if (duplicate is not null && duplicate.Id != agency.Id)
            throw new AgencyCodeAlreadyExistException(cmd.Code);

        agency.Update(
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
            cmd.CityId is null ? null : new CityId(cmd.CityId.Value),
            cmd.SectorId is null ? null : new SectorId(cmd.SectorId.Value),
            cmd.AgencyTypeId is null ? null : new ParamTypeId(cmd.AgencyTypeId.Value),
            cmd.SupportAccountId,
            cmd.PartnerId,
            cmd.IsEnabled);

        await _repo.UpdateAsync(agency, ct);
        return Result.Success(agency.Id.Value);
    }
}