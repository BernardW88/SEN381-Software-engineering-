using Models;
using System.Collections.Generic;

namespace DataAccess
{
    public interface IServiceRequestRepository
    {
        List<ServiceRequest> GetAllRequests();
        List<ServiceRequest> SearchRequests(string searchTerm, string categoryFilter, string statusFilter);
        bool AddRequest(ServiceRequest request);
        bool UpdateRequestStatus(int requestId, string newStatus, string modifiedBy);
    }
}
