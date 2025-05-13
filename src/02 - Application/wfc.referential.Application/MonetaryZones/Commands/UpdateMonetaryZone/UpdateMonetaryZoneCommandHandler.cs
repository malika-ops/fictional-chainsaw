using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;

public class UpdateMonetaryZoneCommandHandler : ICommandHandler<UpdateMonetaryZoneCommand, Result<Guid>>
{
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public UpdateMonetaryZoneCommandHandler(IMonetaryZoneRepository monetaryZoneRepository)
    {
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<Result<Guid>> Handle(UpdateMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var isExist = await _monetaryZoneRepository.GetByCodeAsync(request.Code, cancellationToken);

        var monetaryZone = await _monetaryZoneRepository.GetByIdAsync(new MonetaryZoneId(request.MonetaryZoneId), cancellationToken);

        if (monetaryZone is null)
            throw new KeyNotFoundException("Monetary zone not found");

        if (isExist is not null && !string.Equals(isExist.Code, monetaryZone!.Code, StringComparison.OrdinalIgnoreCase)) 
            throw new CodeAlreadyExistException(request.Code);

        monetaryZone.Update(request.Code, request.Name, request.Description , request.IsEnabled);

        await _monetaryZoneRepository.UpdateMonetaryZoneAsync(monetaryZone, cancellationToken);

        return Result.Success(monetaryZone.Id!.Value);
    }
}