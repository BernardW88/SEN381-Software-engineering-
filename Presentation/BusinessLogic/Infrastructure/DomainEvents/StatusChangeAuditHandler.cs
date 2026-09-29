using BusinessLogic.Infrastructure;

namespace BusinessLogic.Infrastructure.DomainEvents
{
    public sealed class StatusChangeAuditHandler : IDomainEventHandler<ServiceRequestStatusChangedEvent>
    {
        public void Handle(ServiceRequestStatusChangedEvent domainEvent)
        {
            ErrorHandler.LogInfo(
                "Status change requestId=" + domainEvent.RequestId +
                " from='" + domainEvent.PreviousStatus +
                "' to='" + domainEvent.NewStatus +
                "' actor='" + domainEvent.ModifiedBy +
                "' at=" + domainEvent.OccurredAtUtc.ToString("O"));
        }
    }
}
