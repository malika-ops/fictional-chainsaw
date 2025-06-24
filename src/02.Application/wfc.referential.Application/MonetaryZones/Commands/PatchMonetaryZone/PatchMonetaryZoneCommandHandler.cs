using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public class PatchMonetaryZoneCommandHandler(IMonetaryZoneRepository _monetaryZoneRepository) 
    : ICommandHandler<PatchMonetaryZoneCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId);

        var monetaryZone = await _monetaryZoneRepository.GetByIdAsync(monetaryZoneId, cancellationToken) 
            ?? throw new BusinessException("Monetary zone not found");

        if (!string.IsNullOrEmpty(request.Code))
        {
            var isExist = await _monetaryZoneRepository.GetOneByConditionAsync(m=>m.Code == request.Code, cancellationToken);

            if (isExist is not null && !string.Equals(isExist.Code, monetaryZone!.Code, StringComparison.OrdinalIgnoreCase))
                throw new CodeAlreadyExistException(request.Code);
        }

        monetaryZone.Patch(request.Code , request.Name , request.Description , request.IsEnabled);

        await _monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
