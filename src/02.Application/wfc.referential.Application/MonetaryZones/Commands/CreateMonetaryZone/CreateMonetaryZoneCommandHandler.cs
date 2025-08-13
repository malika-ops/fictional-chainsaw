using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Kafka.Producer;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public class CreateMonetaryZoneCommandHandler(
    IMonetaryZoneRepository monetaryZoneRepository,
    IProducerService kafkaProducer,
    ICurrentUserContext userContext) 
    : ICommandHandler<CreateMonetaryZoneCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId ?? "anonymous";
        var traceId = userContext.TraceId ?? Guid.NewGuid().ToString();

        var isExist = await monetaryZoneRepository.GetOneByConditionAsync(m=>m.Code == request.Code, cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        var id = MonetaryZoneId.Of(Guid.NewGuid()); 
        var monetaryZone = MonetaryZone.Create(id, request.Code, request.Name, request.Description);

        await monetaryZoneRepository.AddAsync(monetaryZone, cancellationToken);
        await monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        var auditEvent = new MonetaryZoneCreatedAuditEvent
        {
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

        //await kafkaProducer.ProduceAsync(auditEvent, "auditLogsTopic");

        return Result.Success(monetaryZone.Id!.Value);
    }
}
