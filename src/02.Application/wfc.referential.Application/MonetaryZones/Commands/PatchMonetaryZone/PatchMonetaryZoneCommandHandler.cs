using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public class PatchMonetaryZoneCommandHandler(IMonetaryZoneRepository _monetaryZoneRepository) : ICommandHandler<PatchMonetaryZoneCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var monetaryZone = await _monetaryZoneRepository
            .GetByIdAsync(MonetaryZoneId.Of(request.MonetaryZoneId), cancellationToken);

        if (monetaryZone == null)
            throw new BusinessException("Monetary zone not found");

        if (!string.IsNullOrEmpty(request.Code))
        {
            var isExist = await _monetaryZoneRepository.GetByCodeAsync(request.Code, cancellationToken);

            if (isExist is not null && !string.Equals(isExist.Code, monetaryZone!.Code, StringComparison.OrdinalIgnoreCase))
                throw new CodeAlreadyExistException(request.Code);
        }

        monetaryZone.Patch(request.Code , request.Name , request.Description , request.IsEnabled);

        await _monetaryZoneRepository.UpdateMonetaryZoneAsync(monetaryZone, cancellationToken);
        await _monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(monetaryZone.Id!.Value);
    }
}
