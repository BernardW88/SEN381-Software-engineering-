using Models;
using System;
using System.Collections.Generic;

namespace DataAccess
{
    public interface IServiceRequestRepository
    {
        List<ServiceRequest> GetAllRequests();
        List<ServiceRequest> SearchRequests(string searchTerm, string categoryFilter, string statusFilter);
        bool AddRequest(ServiceRequest request);
        bool UpdateRequestStatus(
            int requestId,
            string expectedCurrentStatus,
            string newStatus,
            string modifiedBy,
            DateTime expectedUpdatedAtUtc,
            DateTime changedAtUtc);
    }
}
