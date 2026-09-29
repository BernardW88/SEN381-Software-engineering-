using Microsoft.Data.SqlClient;
using Models;
using System;
using System.Collections.Generic;

namespace DataAccess
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        public List<ServiceRequest> GetAllRequests()
        {
            return SearchRequests("", "All Categories", "All Statuses");
        }

        public List<ServiceRequest> SearchRequests(string searchTerm, string categoryFilter, string statusFilter)
        {
            List<ServiceRequest> requests = new List<ServiceRequest>();

            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"SELECT RequestID, Title, Category, Location, Description, Status, Priority, DateCreated, LastModifiedBy 
                                 FROM ServiceRequests 
                                 WHERE (Title LIKE @Search OR Location LIKE @Search OR Description LIKE @Search)";

                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
                {
                    query += " AND Category = @Category";
                }

                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All Statuses")
                {
                    query += " AND Status = @Status";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                    if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
                        cmd.Parameters.AddWithValue("@Category", categoryFilter);
                    if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All Statuses")
                        cmd.Parameters.AddWithValue("@Status", statusFilter);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new ServiceRequest
                            {
                                RequestID = Convert.ToInt32(reader["RequestID"]),
                                Title = reader["Title"].ToString(),
                                Category = reader["Category"].ToString(),
                                Location = reader["Location"].ToString(),
                                Description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString(),
                                Status = reader["Status"].ToString(),
                                Priority = reader["Priority"].ToString(),
                                DateCreated = Convert.ToDateTime(reader["DateCreated"]),
                                LastModifiedBy = reader["LastModifiedBy"] == DBNull.Value ? "System" : reader["LastModifiedBy"].ToString()
                            });
                        }
                    }
                }
            }
            return requests;
        }

        public bool AddRequest(ServiceRequest request)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"INSERT INTO ServiceRequests (Title, Category, Location, Description, Status, Priority, DateCreated, LastModifiedBy) 
                                 VALUES (@Title, @Category, @Location, @Description, @Status, @Priority, @DateCreated, @LastModifiedBy)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", request.Title);
                    cmd.Parameters.AddWithValue("@Category", request.Category);
                    cmd.Parameters.AddWithValue("@Location", request.Location);
                    cmd.Parameters.AddWithValue("@Description", request.Description ?? "");
                    cmd.Parameters.AddWithValue("@Status", request.Status);
                    cmd.Parameters.AddWithValue("@Priority", request.Priority);
                    cmd.Parameters.AddWithValue("@DateCreated", request.DateCreated);
                    cmd.Parameters.AddWithValue("@LastModifiedBy", request.LastModifiedBy);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateRequestStatus(int requestId, string newStatus, string modifiedBy)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"UPDATE ServiceRequests 
                                 SET Status = @Status, LastModifiedBy = @LastModifiedBy 
                                 WHERE RequestID = @RequestID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@LastModifiedBy", modifiedBy);
                    cmd.Parameters.AddWithValue("@RequestID", requestId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}