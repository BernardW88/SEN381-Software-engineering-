using System;

namespace Models
{
    public class Tool
    {
        public int ToolID { get; set; }
        public string ToolName { get; set; }
        public string Category { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime DateAdded { get; set; }
        public string LastModifiedBy { get; set; }

        // Computed helper property for clean UI binding
        public string AvailabilityStatus => IsAvailable ? "Available" : "Checked Out";
    }
}