using DataAccess;
using Models;
using System;

namespace BusinessLogic.Escalation
{
    // Escalate immediately if priority is Critical or High.
    public class ImmediateEscalationStrategy : IEscalationStrategy
    {
        private readonly IServiceRequestRepository _repo;
        private readonly string _escalatedStatus;

        public ImmediateEscalationStrategy(IServiceRequestRepository repo = null, string escalatedStatus = "Escalated")
        {
            _repo = repo ?? new ServiceRequestRepository();
            _escalatedStatus = escalatedStatus;
        }

        public bool ShouldEscalate(ServiceRequest request)
        {
            if (request == null) return false;
            return string.Equals(request.Priority, "Critical", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.Priority, "High", StringComparison.OrdinalIgnoreCase);
        }

        public bool Escalate(ServiceRequest request)
        {
            if (request == null) return false;

            if (!ShouldEscalate(request)) return false;

            try
            {
                // Update status via repository and mark last modified by system.
                return _repo.UpdateRequestStatus(request.RequestID, _escalatedStatus, "System-Escalation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ImmediateEscalation failed: {ex.Message}");
                return false;
            }
        }
    }
}
