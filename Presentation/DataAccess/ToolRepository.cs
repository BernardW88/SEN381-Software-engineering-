using Microsoft.Data.SqlClient;
using Models;
using System;
using System.Collections.Generic;

namespace DataAccess
{
    public class ToolRepository : IToolRepository
    {
        public List<Tool> GetAllTools()
        {
            return SearchTools("", "All Categories");
        }

        public List<Tool> SearchTools(string searchTerm, string categoryFilter)
        {
            List<Tool> tools = new List<Tool>();

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"SELECT ToolID, ToolName, Category, IsAvailable, DateAdded, LastModifiedBy 
                                 FROM Tools 
                                 WHERE (ToolName LIKE @Search OR Category LIKE @Search)";

                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
                {
                    query += " AND Category = @Category";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                    if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
                    {
                        cmd.Parameters.AddWithValue("@Category", categoryFilter);
                    }

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tools.Add(new Tool
                            {
                                ToolID = Convert.ToInt32(reader["ToolID"]),
                                ToolName = reader["ToolName"].ToString(),
                                Category = reader["Category"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                DateAdded = Convert.ToDateTime(reader["DateAdded"]),
                                LastModifiedBy = reader["LastModifiedBy"] == DBNull.Value ? "N/A" : reader["LastModifiedBy"].ToString()
                            });
                        }
                    }
                }
            }
            return tools;
        }

        public bool AddTool(Tool tool)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"INSERT INTO Tools (ToolName, Category, IsAvailable, DateAdded, LastModifiedBy) 
                                 VALUES (@ToolName, @Category, @IsAvailable, @DateAdded, @LastModifiedBy)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ToolName", tool.ToolName);
                    cmd.Parameters.AddWithValue("@Category", tool.Category);
                    cmd.Parameters.AddWithValue("@IsAvailable", tool.IsAvailable);
                    cmd.Parameters.AddWithValue("@DateAdded", tool.DateAdded);
                    cmd.Parameters.AddWithValue("@LastModifiedBy", tool.LastModifiedBy);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateToolAvailability(int toolId, bool isAvailable, string modifiedBy)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"UPDATE Tools 
                                 SET IsAvailable = @IsAvailable, LastModifiedBy = @LastModifiedBy 
                                 WHERE ToolID = @ToolID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IsAvailable", isAvailable);
                    cmd.Parameters.AddWithValue("@LastModifiedBy", modifiedBy);
                    cmd.Parameters.AddWithValue("@ToolID", toolId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
