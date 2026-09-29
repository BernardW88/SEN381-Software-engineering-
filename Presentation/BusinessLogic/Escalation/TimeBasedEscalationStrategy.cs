using DataAccess;
using Models;
using System;

namespace BusinessLogic.Escalation
{
    // Escalate if request is older than a configured threshold and still in a non-terminal state.
    public class TimeBasedEscalationStrategy : IEscalationStrategy
    {
        private readonly IServiceRequestRepository _repo;
        private readonly TimeSpan _threshold;
        private readonly string _escalatedStatus;

        public TimeBasedEscalationStrategy(TimeSpan? threshold = null, IServiceRequestRepository repo = null, string escalatedStatus = "Escalated")
        {
            _repo = repo ?? new ServiceRequestRepository();
            _threshold = threshold ?? TimeSpan.FromDays(2);
            _escalatedStatus = escalatedStatus;
        }

        public bool ShouldEscalate(ServiceRequest request)
        {
            if (request == null) return false;
            if (request.Status == "Closed" || request.Status == "Resolved") return false;

            return (DateTime.Now - request.DateCreated) > _threshold;
        }

        public bool Escalate(ServiceRequest request)
        {
            if (request == null) return false;
            if (!ShouldEscalate(request)) return false;

            try
            {
                return _repo.UpdateRequestStatus(request.RequestID, _escalatedStatus, "System-TimeEscalation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TimeBasedEscalation failed: {ex.Message}");
                return false;
            }
        }
    }
}
