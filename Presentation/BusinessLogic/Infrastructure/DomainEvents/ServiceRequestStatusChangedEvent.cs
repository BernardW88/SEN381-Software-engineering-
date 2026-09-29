using System;

namespace BusinessLogic.Infrastructure.DomainEvents
{
    public sealed class ServiceRequestStatusChangedEvent : IDomainEvent
    {
        public ServiceRequestStatusChangedEvent(
            int requestId,
            string previousStatus,
            string newStatus,
            string modifiedBy,
            DateTime occurredAtUtc)
        {
            RequestId = requestId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            ModifiedBy = modifiedBy;
            OccurredAtUtc = occurredAtUtc;
        }

        public int RequestId { get; }
        public string PreviousStatus { get; }
        public string NewStatus { get; }
        public string ModifiedBy { get; }
        public DateTime OccurredAtUtc { get; }
    }
}
