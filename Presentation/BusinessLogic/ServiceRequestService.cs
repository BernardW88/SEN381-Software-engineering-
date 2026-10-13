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
        private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pothole",
            "Water Outage",
            "Electricity",
            "Waste"
        };

        private static readonly HashSet<string> AllowedPriorities = new(StringComparer.OrdinalIgnoreCase)
        {
            "Low",
            "Medium",
            "High",
            "Critical"
        };

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
            {
                ErrorHandler.LogWarning($"CreateNewRequest rejected: empty title (user={currentUser})");
                throw new ServiceRequestDomainException("Service request title is required.");
            }

            if (string.IsNullOrWhiteSpace(category) || category == "Select Category" || !AllowedCategories.Contains(category.Trim()))
            {
                ErrorHandler.LogWarning($"CreateNewRequest rejected: invalid category '{category}' (user={currentUser})");
                throw new ServiceRequestDomainException("Please select a valid service category.");
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                ErrorHandler.LogWarning($"CreateNewRequest rejected: empty location (user={currentUser})");
                throw new ServiceRequestDomainException("Issue location is required.");
            }

            string normalizedPriority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority.Trim();
            if (!AllowedPriorities.Contains(normalizedPriority))
            {
                ErrorHandler.LogWarning($"CreateNewRequest rejected: invalid priority '{priority}' (user={currentUser})");
                throw new ServiceRequestDomainException("Invalid priority selected.");
            }

            DateTime nowUtc = DateTime.UtcNow;
            ServiceRequest request = new ServiceRequest
            {
                Title = title.Trim(),
                Category = category.Trim(),
                Location = location.Trim(),
                Description = description ?? "",
                Priority = normalizedPriority,
                Status = "Submitted",
                DateCreated = nowUtc,
                UpdatedAt = nowUtc,
                LastModifiedBy = string.IsNullOrWhiteSpace(currentUser) ? "System" : currentUser.Trim()
            };

            bool added = _requestRepository.AddRequest(request);
            if (!added)
            {
                ErrorHandler.LogError($"CreateNewRequest repository insert failed (user={currentUser}, title={request.Title})");
                return null;
            }

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
                ErrorHandler.LogWarning($"AdvanceRequestStatus rejected: invalid requestId={requestId} (user={currentUser})");
                throw new ServiceRequestDomainException("Invalid service request selected.");
            }

            var existing = GetRequestById(requestId);
            if (existing == null)
            {
                ErrorHandler.LogWarning($"AdvanceRequestStatus rejected: request not found id={requestId} (user={currentUser})");
                throw new ServiceRequestDomainException("Service request not found.");
            }

            string expectedCurrent = existing.Status;
            if (!string.Equals(currentStatus, expectedCurrent, StringComparison.OrdinalIgnoreCase))
            {
                ErrorHandler.LogWarning($"AdvanceRequestStatus stale UI state for requestId={requestId}: UI='{currentStatus}', DB='{expectedCurrent}'");
                throw new ServiceRequestDomainException("Request status changed by another operator. Please refresh and try again.");
            }

            if (!IsValidStateTransition(expectedCurrent, targetStatus))
            {
                ErrorHandler.LogWarning($"Invalid transition attempted requestId={requestId} from '{expectedCurrent}' to '{targetStatus}' (user={currentUser})");
                throw new ServiceRequestDomainException($"Invalid status transition from '{expectedCurrent}' to '{targetStatus}'. Allowed next step only.");
            }

            DateTime changedAtUtc = DateTime.UtcNow;
            string actor = string.IsNullOrWhiteSpace(currentUser) ? "System" : currentUser.Trim();

            try
            {
                bool result = _requestRepository.UpdateRequestStatus(
                    requestId,
                    expectedCurrent,
                    targetStatus,
                    actor,
                    existing.UpdatedAt,
                    changedAtUtc);

                if (!result)
                {
                    ErrorHandler.LogWarning($"AdvanceRequestStatus concurrency conflict for requestId={requestId}");
                    throw new ServiceRequestDomainException("This request was updated by another operator. Refresh and retry.");
                }

                _eventPublisher.Publish(new ServiceRequestStatusChangedEvent(
                    requestId,
                    expectedCurrent,
                    targetStatus,
                    actor,
                    changedAtUtc));

                return true;
            }
            catch (ServiceRequestDomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError($"AdvanceRequestStatus exception for requestId={requestId}: {ex.Message} (user={currentUser})");
                throw;
            }
        }

        private bool IsValidStateTransition(string current, string target)
        {
            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(target)) return false;
            if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase)) return false;

            // Strict sequential lifecycle rules: Submitted -> In Progress -> Resolved -> Closed
            return current switch
            {
                "Submitted" => string.Equals(target, "In Progress", StringComparison.OrdinalIgnoreCase),
                "In Progress" => string.Equals(target, "Resolved", StringComparison.OrdinalIgnoreCase),
                "Resolved" => string.Equals(target, "Closed", StringComparison.OrdinalIgnoreCase),
                "Closed" => false,
                _ => false
            };
        }

        // Expose valid next statuses for the UI to present choices
        public List<string> GetValidNextStatuses(string current)
        {
            var candidates = new List<string> { "Submitted", "In Progress", "Resolved", "Closed" };
            var result = new List<string>();
            foreach (var c in candidates)
            {
                if (IsValidStateTransition(current, c))
                    result.Add(c);
            }
            return result;
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