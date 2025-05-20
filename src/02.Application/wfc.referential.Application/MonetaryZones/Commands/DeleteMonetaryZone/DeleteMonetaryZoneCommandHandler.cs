using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;

public class DeleteMonetaryZoneCommandHandler(IMonetaryZoneRepository _monetaryZoneRepository) : ICommandHandler<DeleteMonetaryZoneCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var monetaryZone = await _monetaryZoneRepository.GetByIdAsync(MonetaryZoneId.Of(request.MonetaryZoneId), cancellationToken);

        if (monetaryZone == null)
            throw new InvalidDeletingException("Monetary zone not found");

        if (monetaryZone.Countries.Count > 0)
            throw new InvalidDeletingException("Can not delete a Monetary Zone with existing Countries");

        monetaryZone.Disable();

        await _monetaryZoneRepository.UpdateMonetaryZoneAsync(monetaryZone, cancellationToken);
        await _monetaryZoneRepository.SaveChangesAsync(cancellationToken);


        return Result.Success(true);
    }
}



