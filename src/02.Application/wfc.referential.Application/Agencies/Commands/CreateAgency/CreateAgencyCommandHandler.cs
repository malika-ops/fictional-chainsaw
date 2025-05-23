using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyAggregate.Exceptions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Agencies.Commands.CreateAgency;

public class CreateAgencyCommandHandler : ICommandHandler<CreateAgencyCommand, Result<Guid>>
{
    private readonly IAgencyRepository _agencyRepository;

    public CreateAgencyCommandHandler(IAgencyRepository repository)
        => _agencyRepository = repository;

    // test
    public async Task<Result<Guid>> Handle(CreateAgencyCommand command, CancellationToken ct)
    {
        var existingAgency = await _agencyRepository.GetOneByConditionAsync(a => a.Code == command.Code, ct);
        if (existingAgency is not null)
            throw new AgencyCodeAlreadyExistException(command.Code);

        var agency = Agency.Create(
            AgencyId.Of(Guid.NewGuid()),
            command.Code,
            command.Name,
            command.Abbreviation,
            command.Address1,
            command.Address2,
            command.Phone,
            command.Fax,
            command.AccountingSheetName,
            command.AccountingAccountNumber,
            command.MoneyGramReferenceNumber,
            command.MoneyGramPassword,
            command.PostalCode,
            command.PermissionOfficeChange,
            command.Latitude,
            command.Longitude,
            command.CityId.HasValue ? CityId.Of(command.CityId.Value) : null,
            command.SectorId.HasValue ? SectorId.Of(command.SectorId.Value) : null,
            command.AgencyTypeId.HasValue ? ParamTypeId.Of(command.AgencyTypeId.Value) : null,
            command.SupportAccountId,
            command.PartnerId);

        await _agencyRepository.AddAsync(agency, ct);
        await _agencyRepository.SaveChangesAsync(ct);
        return Result.Success(agency.Id!.Value);
    }
}
