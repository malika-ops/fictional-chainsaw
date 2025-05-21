using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public class CreateMonetaryZoneCommandHandler(IMonetaryZoneRepository _monetaryZoneRepository) : ICommandHandler<CreateMonetaryZoneCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var isExist = await _monetaryZoneRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        var id = MonetaryZoneId.Of(Guid.NewGuid()); 
        var monetaryZone = MonetaryZone.Create(id, request.Code, request.Name, request.Description);

        await _monetaryZoneRepository.AddMonetaryZoneAsync(monetaryZone, cancellationToken);
        await _monetaryZoneRepository.SaveChangesAsync(cancellationToken);


        return Result.Success(monetaryZone.Id!.Value);

    }
}
