using System;

namespace Models
{
    public class ServiceRequest
    {
        public int RequestID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Submitted, In Progress, Resolved, Closed
        public string Priority { get; set; } // Low, Medium, High, Critical
        public DateTime DateCreated { get; set; }
        public string LastModifiedBy { get; set; }
    }
}