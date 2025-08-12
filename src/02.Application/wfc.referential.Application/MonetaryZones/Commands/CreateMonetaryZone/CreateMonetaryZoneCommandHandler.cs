using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Kafka.Producer;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;

public class CreateMonetaryZoneCommandHandler(IMonetaryZoneRepository _monetaryZoneRepository,IProducerService _kafkaProducer,ICurrentUserContext _userContext) 
    : ICommandHandler<CreateMonetaryZoneCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId ?? "anonymous";
        var traceId = _userContext.TraceId ?? Guid.NewGuid().ToString();

        var isExist = await _monetaryZoneRepository.GetOneByConditionAsync(m=>m.Code == request.Code, cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        var id = MonetaryZoneId.Of(Guid.NewGuid()); 
        var monetaryZone = MonetaryZone.Create(id, request.Code, request.Name, request.Description);

        await _monetaryZoneRepository.AddAsync(monetaryZone, cancellationToken);
        await _monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        var auditEvent = new MonetaryZoneCreatedAuditEvent
        {
            NewValueJson = monetaryZone,
            OldValueJson = null,
            Timestamp = DateTime.UtcNow,
            EntityId = monetaryZone.Id!.ToString(),
            MetadataJson = new
            {
                ip = _userContext.IpAddress,
                TraceId = traceId
            },
            
        };

        //await _kafkaProducer.ProduceAsync(auditEvent, "auditLogsTopic");

        return Result.Success(monetaryZone.Id!.Value);

    }
}
