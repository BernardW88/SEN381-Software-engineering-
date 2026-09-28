using DataAccess;
using Models;
using System;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class ToolService
    {
        private readonly IToolRepository _toolRepository;

        public ToolService(IToolRepository toolRepository = null)
        {
            _toolRepository = toolRepository ?? new ToolRepository();
        }

        public List<Tool> GetFilteredInventory(string searchTerm, string category)
        {
            return _toolRepository.SearchTools(searchTerm ?? "", category ?? "All Categories");
        }

        public bool AddNewTool(string name, string category, string currentUsername)
        {
            // Business Validation Rules
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tool name is required.");

            if (string.IsNullOrWhiteSpace(category) || category == "Select Category")
                throw new ArgumentException("Please select a valid tool category.");

            if (string.IsNullOrWhiteSpace(currentUsername))
                currentUsername = "System";

            Tool tool = new Tool
            {
                ToolName = name.Trim(),
                Category = category.Trim(),
                IsAvailable = true,
                DateAdded = DateTime.Now,
                LastModifiedBy = currentUsername
            };

            return _toolRepository.AddTool(tool);
        }

        public bool ToggleBorrowReturn(int toolId, bool currentStatus, string currentUsername)
        {
            if (toolId <= 0)
                throw new ArgumentException("Invalid tool selected.");

            if (string.IsNullOrWhiteSpace(currentUsername))
                currentUsername = "System";

            return _toolRepository.UpdateToolAvailability(toolId, !currentStatus, currentUsername);
        }

        // Calculates dynamic live metrics for the dashboard summary header
        public (int Total, int Available, int CheckedOut) GetInventoryStats()
        {
            List<Tool> allTools = _toolRepository.GetAllTools();
            int total = allTools.Count;
            int available = allTools.FindAll(t => t.IsAvailable).Count;
            int checkedOut = total - available;

            return (total, available, checkedOut);
        }
    }
}
