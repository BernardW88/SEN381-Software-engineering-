using BusinessLogic.Infrastructure;
using System;
using System.IO;
using System.Text;

namespace BusinessLogic.Infrastructure.DomainEvents
{
    public sealed class StatusChangeAuditHandler : IDomainEventHandler<ServiceRequestStatusChangedEvent>
    {
        private static readonly object Sync = new();
        private static readonly string AuditFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? ".", "status-audit.log");

        public void Handle(ServiceRequestStatusChangedEvent domainEvent)
        {
            string auditRecord =
                "AUDIT|RequestID=" + domainEvent.RequestId +
                "|PreviousStatus=" + domainEvent.PreviousStatus +
                "|NewStatus=" + domainEvent.NewStatus +
                "|ChangedBy=" + domainEvent.ModifiedBy +
                "|TimestampUtc=" + domainEvent.OccurredAtUtc.ToString("O");

            ErrorHandler.LogInfo(auditRecord);
            AppendAuditRecord(auditRecord);
        }

        private static void AppendAuditRecord(string record)
        {
            try
            {
                lock (Sync)
                {
                    File.AppendAllText(AuditFilePath, record + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("StatusChangeAuditHandler append failed: " + ex.Message);
            }
        }
    }
}
