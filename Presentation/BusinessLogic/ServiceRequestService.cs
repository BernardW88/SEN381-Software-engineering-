using DataAccess;
using Models;
using System;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class ServiceRequestService
    {
        private readonly IServiceRequestRepository _requestRepository;

        public ServiceRequestService(IServiceRequestRepository requestRepository = null)
        {
            _requestRepository = requestRepository ?? new ServiceRequestRepository();
        }

        public List<ServiceRequest> GetFilteredRequests(string search, string category, string status)
        {
            return _requestRepository.SearchRequests(search ?? "", category ?? "All Categories", status ?? "All Statuses");
        }

        public bool CreateNewRequest(string title, string category, string location, string description, string priority, string currentUser)
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

            return _requestRepository.AddRequest(request);
        }

        // M2 Design Pattern Implementation: Lifecycle State Control Strategy
        public bool AdvanceRequestStatus(int requestId, string currentStatus, string targetStatus, string currentUser)
        {
            if (requestId <= 0)
                throw new ArgumentException("Invalid service request selected.");

            // Enforce valid lifecycle state progression rules
            if (!IsValidStateTransition(currentStatus, targetStatus))
            {
                throw new InvalidOperationException($"Invalid status transition! Cannot change request state directly from '{currentStatus}' to '{targetStatus}'.");
            }

            return _requestRepository.UpdateRequestStatus(requestId, targetStatus, currentUser ?? "System");
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