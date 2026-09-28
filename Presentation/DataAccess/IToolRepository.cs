using Models;
using System.Collections.Generic;

namespace DataAccess
{
    public interface IToolRepository
    {
        List<Tool> GetAllTools();
        List<Tool> SearchTools(string searchTerm, string categoryFilter);
        bool AddTool(Tool tool);
        bool UpdateToolAvailability(int toolId, bool isAvailable, string modifiedBy);
    }
}