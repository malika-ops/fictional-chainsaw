using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public class PatchMonetaryZoneCommandHandler : ICommandHandler<PatchMonetaryZoneCommand, Result<Guid>>
{
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public PatchMonetaryZoneCommandHandler(IMonetaryZoneRepository monetaryZoneRepository)
    {
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<Result<Guid>> Handle(PatchMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var monetaryZone = await _monetaryZoneRepository
            .GetByIdAsync(MonetaryZoneId.Of(request.MonetaryZoneId), cancellationToken);

        if (monetaryZone == null)
            throw new KeyNotFoundException("Monetary zone not found");

        if (!string.IsNullOrEmpty(request.Code))
        {
            var isExist = await _monetaryZoneRepository.GetByCodeAsync(request.Code, cancellationToken);

            if (isExist is not null && !string.Equals(isExist.Code, monetaryZone!.Code, StringComparison.OrdinalIgnoreCase))
                throw new CodeAlreadyExistException(request.Code);
        }

        request.Adapt(monetaryZone);
        monetaryZone.Patch();
        await _monetaryZoneRepository.UpdateMonetaryZoneAsync(monetaryZone, cancellationToken);

        return Result.Success(monetaryZone.Id!.Value);
    }
}
