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

    public async Task<Result<Guid>> Handle(CreateAgencyCommand c, CancellationToken ct)
    {
        if (await _agencyRepository.GetByCodeAsync(c.Code, ct) is not null)
            throw new AgencyCodeAlreadyExistException(c.Code);

        var agency = Agency.Create(
            AgencyId.Of(Guid.NewGuid()),
            c.Code,
            c.Name,
            c.Abbreviation,
            c.Address1,
            c.Address2,
            c.Phone,
            c.Fax,
            c.AccountingSheetName,
            c.AccountingAccountNumber,
            c.MoneyGramReferenceNumber,
            c.MoneyGramPassword,
            c.PostalCode,
            c.PermissionOfficeChange,
            c.Latitude,
            c.Longitude,
            true,
            c.CityId.HasValue ? CityId.Of(c.CityId.Value) : null,
            c.SectorId.HasValue ? SectorId.Of(c.SectorId.Value) : null,
            c.AgencyTypeId.HasValue ? ParamTypeId.Of(c.AgencyTypeId.Value) : null,
            c.SupportAccountId,
            c.PartnerId);

        await _agencyRepository.AddAsync(agency, ct);
        return Result.Success(agency.Id!.Value);
    }
}
