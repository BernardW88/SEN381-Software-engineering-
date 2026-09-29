using DataAccess;
using Models;
using System;
using System.Collections.Generic;
using BusinessLogic.Infrastructure;
using BusinessLogic.Infrastructure.DomainEvents;

namespace BusinessLogic
{
    public class ServiceRequestService
    {
        private readonly IServiceRequestRepository _requestRepository;
        private readonly IDomainEventPublisher _eventPublisher;

        public ServiceRequestService(
            IServiceRequestRepository requestRepository = null,
            IDomainEventPublisher eventPublisher = null)
        {
            _requestRepository = requestRepository ?? new ServiceRequestRepository();
            _eventPublisher = eventPublisher ?? new InMemoryDomainEventPublisher();
            _eventPublisher.Subscribe(new StatusChangeAuditHandler());
        }

        public List<ServiceRequest> GetFilteredRequests(string search, string category, string status)
        {
            return _requestRepository.SearchRequests(search ?? "", category ?? "All Categories", status ?? "All Statuses");
        }

        public ServiceRequest CreateNewRequest(string title, string category, string location, string description, string priority, string currentUser)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Service request title is required.");

            if (string.IsNullOrWhiteSpace(category) || category == "Select Category")
                throw new ArgumentException("Please select a valid service category.");

            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Issue location is required.");

            ServiceRequest request = new ServiceRequest
            {
                Title = title.Trim(),
                Category = category.Trim(),
                Location = location.Trim(),
                Description = description ?? "",
                Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority,
                Status = "Submitted", // Default lifecycle state
                DateCreated = DateTime.Now,
                LastModifiedBy = currentUser ?? "System"
            };

            bool added = _requestRepository.AddRequest(request);
            if (!added) return null;

            // Attempt to locate the newly created request in the repository.
            // We use a best-effort strategy: match by title and location and take the most recently created.
            var all = _requestRepository.GetAllRequests();
            ServiceRequest created = all.FindLast(r => r.Title == request.Title && r.Location == request.Location);
            return created;
        }

        public ServiceRequest GetRequestById(int id)
        {
            if (id <= 0) return null;
            var all = _requestRepository.GetAllRequests();
            return all.Find(r => r.RequestID == id);
        }

        // M2 Design Pattern Implementation: Lifecycle State Control Strategy
        public bool AdvanceRequestStatus(int requestId, string currentStatus, string targetStatus, string currentUser)
        {
            if (requestId <= 0)
            {
                ErrorHandler.LogWarning($"AdvanceRequestStatus called with invalid requestId={requestId} (user={currentUser})");
                throw new ArgumentException("Invalid service request selected.");
            }

            // Verify the request exists
            var existing = GetRequestById(requestId);
            if (existing == null)
            {
                ErrorHandler.LogWarning($"AdvanceRequestStatus: request not found id={requestId} (user={currentUser})");
                throw new ArgumentException("Service request not found.");
            }

            // Enforce valid lifecycle state progression rules
            if (!IsValidStateTransition(currentStatus, targetStatus))
            {
                ErrorHandler.LogWarning($"Invalid state transition attempted for requestId={requestId} from '{currentStatus}' to '{targetStatus}' (user={currentUser})");
                throw new InvalidOperationException($"Invalid status transition! Cannot change request state directly from '{currentStatus}' to '{targetStatus}'.");
            }

            try
            {
                bool result = _requestRepository.UpdateRequestStatus(requestId, targetStatus, currentUser ?? "System");
                if (!result)
                    ErrorHandler.LogError($"AdvanceRequestStatus failed to update repository for requestId={requestId} (user={currentUser})");
                else
                    _eventPublisher.Publish(new ServiceRequestStatusChangedEvent(
                        requestId,
                        currentStatus,
                        targetStatus,
                        currentUser ?? "System",
                        DateTime.UtcNow));
                return result;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"AdvanceRequestStatus exception for requestId={requestId}: {ex.Message} (user={currentUser})");
                throw;
            }
        }

        private bool IsValidStateTransition(string current, string target)
        {
            if (current == target) return false;

            // Define Business State Transition Rules
            return current switch
            {
                "Submitted" => target == "In Progress" || target == "Closed",
                "In Progress" => target == "Resolved",
                "Resolved" => target == "Closed" || target == "In Progress", // Can reopen if unresolved
                "Closed" => false, // Terminal state
                _ => false
            };
        }

        // Dynamic Aggregation Metrics for Dashboard Header
        public (int Total, int Pending, int InProgress, int Resolved) GetDashboardMetrics()
        {
            List<ServiceRequest> all = _requestRepository.GetAllRequests();
            int total = all.Count;
            int pending = all.FindAll(r => r.Status == "Submitted").Count;
            int inProgress = all.FindAll(r => r.Status == "In Progress").Count;
            int resolved = all.FindAll(r => r.Status == "Resolved" || r.Status == "Closed").Count;

            return (total, pending, inProgress, resolved);
        }
    }
}