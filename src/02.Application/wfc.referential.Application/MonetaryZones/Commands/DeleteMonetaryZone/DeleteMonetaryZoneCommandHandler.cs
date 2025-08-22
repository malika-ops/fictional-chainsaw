using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Kafka.Producer;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;

public class DeleteMonetaryZoneCommandHandler(
    IMonetaryZoneRepository monetaryZoneRepository,
    IProducerService kafkaProducer,
    ICurrentUserContext userContext) 
    : ICommandHandler<DeleteMonetaryZoneCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId ?? "anonymous";
        var traceId = userContext.TraceId ?? Guid.NewGuid().ToString();

        var monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId);

        var monetaryZone = await monetaryZoneRepository.GetByIdWithIncludesAsync(monetaryZoneId, 
            cancellationToken, 
            mz => mz.Countries) 
            ?? throw new InvalidDeletingException("Monetary zone not found");

        if (monetaryZone.Countries.Count > 0)
            throw new InvalidDeletingException("Can not delete a Monetary Zone with existing Countries");

        monetaryZone.Disable();

        await monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        var auditEvent = new MonetaryZoneDisabledAuditEvent
        {
            TraceId = traceId,
            NewValueJson = monetaryZone,
            OldValueJson = null,
            Timestamp = DateTime.UtcNow,
            EntityId = monetaryZone.Id!.ToString(),
            MetadataJson = new
            {
                ip = userContext.IpAddress,
                TraceId = traceId
            },

        };

        await kafkaProducer.ProduceAsync(auditEvent);

        return Result.Success(true);
    }
}



