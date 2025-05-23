using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyAggregate.Exceptions;

namespace wfc.referential.Application.Agencies.Commands.UpdateAgency;

public class UpdateAgencyCommandHandler
    : ICommandHandler<UpdateAgencyCommand, Result<bool>>
{
    private readonly IAgencyRepository _repo;

    public UpdateAgencyCommandHandler(IAgencyRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateAgencyCommand cmd, CancellationToken ct)
    {
        var agency = await _repo.GetByIdAsync(AgencyId.Of(cmd.AgencyId), ct);
        if (agency is null)
            throw new BusinessException($"Agency [{cmd.AgencyId}] not found.");

        // uniqueness on Code
        var duplicate = await _repo.GetOneByConditionAsync(a => a.Code == cmd.Code, ct);
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
            cmd.CityId,
            cmd.SectorId,
            cmd.AgencyTypeId,
            cmd.SupportAccountId,
            cmd.PartnerId,
            cmd.IsEnabled);

        _repo.Update(agency);
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}