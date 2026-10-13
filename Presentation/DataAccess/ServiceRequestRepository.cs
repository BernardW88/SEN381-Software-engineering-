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
                string query = @"SELECT RequestID, Title, Category, Location, Description, Status, Priority, DateCreated, UpdatedAt, LastModifiedBy 
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
                    cmd.Parameters.Add("@Search", System.Data.SqlDbType.NVarChar, 300).Value = $"%{searchTerm}%";
                    if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
                        cmd.Parameters.Add("@Category", System.Data.SqlDbType.NVarChar, 100).Value = categoryFilter;
                    if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All Statuses")
                        cmd.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 100).Value = statusFilter;

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
                                UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                                    ? Convert.ToDateTime(reader["DateCreated"])
                                    : Convert.ToDateTime(reader["UpdatedAt"]),
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
                string query = @"INSERT INTO ServiceRequests (Title, Category, Location, Description, Status, Priority, DateCreated, UpdatedAt, LastModifiedBy) 
                                 VALUES (@Title, @Category, @Location, @Description, @Status, @Priority, @DateCreated, @UpdatedAt, @LastModifiedBy)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar, 200).Value = request.Title;
                    cmd.Parameters.Add("@Category", System.Data.SqlDbType.NVarChar, 100).Value = request.Category;
                    cmd.Parameters.Add("@Location", System.Data.SqlDbType.NVarChar, 200).Value = request.Location;
                    cmd.Parameters.Add("@Description", System.Data.SqlDbType.NVarChar, -1).Value = request.Description ?? "";
                    cmd.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 100).Value = request.Status;
                    cmd.Parameters.Add("@Priority", System.Data.SqlDbType.NVarChar, 50).Value = request.Priority;
                    cmd.Parameters.Add("@DateCreated", System.Data.SqlDbType.DateTime2).Value = request.DateCreated;
                    cmd.Parameters.Add("@UpdatedAt", System.Data.SqlDbType.DateTime2).Value = request.UpdatedAt == default ? DateTime.UtcNow : request.UpdatedAt;
                    cmd.Parameters.Add("@LastModifiedBy", System.Data.SqlDbType.NVarChar, 100).Value = request.LastModifiedBy ?? "System";

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateRequestStatus(
            int requestId,
            string expectedCurrentStatus,
            string newStatus,
            string modifiedBy,
            DateTime expectedUpdatedAtUtc,
            DateTime changedAtUtc)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = @"UPDATE ServiceRequests
                                               SET Status = @NewStatus,
                                                   LastModifiedBy = @LastModifiedBy,
                                                   UpdatedAt = @ChangedAtUtc
                                               WHERE RequestID = @RequestID
                                                 AND Status = @ExpectedCurrentStatus
                                                 AND UpdatedAt = @ExpectedUpdatedAtUtc";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, tx))
                        {
                            updateCmd.Parameters.Add("@NewStatus", System.Data.SqlDbType.NVarChar, 100).Value = newStatus;
                            updateCmd.Parameters.Add("@LastModifiedBy", System.Data.SqlDbType.NVarChar, 100).Value = modifiedBy ?? "System";
                            updateCmd.Parameters.Add("@ChangedAtUtc", System.Data.SqlDbType.DateTime2).Value = changedAtUtc;
                            updateCmd.Parameters.Add("@RequestID", System.Data.SqlDbType.Int).Value = requestId;
                            updateCmd.Parameters.Add("@ExpectedCurrentStatus", System.Data.SqlDbType.NVarChar, 100).Value = expectedCurrentStatus;
                            updateCmd.Parameters.Add("@ExpectedUpdatedAtUtc", System.Data.SqlDbType.DateTime2).Value = expectedUpdatedAtUtc;

                            int updated = updateCmd.ExecuteNonQuery();
                            if (updated == 0)
                            {
                                tx.Rollback();
                                return false;
                            }
                        }

                        string ensureAuditTableQuery = @"IF OBJECT_ID('dbo.ServiceRequestStatusAudit', 'U') IS NULL
                                                         BEGIN
                                                             CREATE TABLE dbo.ServiceRequestStatusAudit
                                                             (
                                                                 AuditId INT IDENTITY(1,1) PRIMARY KEY,
                                                                 RequestID INT NOT NULL,
                                                                 PreviousStatus NVARCHAR(100) NOT NULL,
                                                                 NewStatus NVARCHAR(100) NOT NULL,
                                                                 ChangedBy NVARCHAR(100) NOT NULL,
                                                                 ChangedAtUtc DATETIME2 NOT NULL
                                                             )
                                                         END";

                        using (SqlCommand ensureAuditCmd = new SqlCommand(ensureAuditTableQuery, conn, tx))
                        {
                            ensureAuditCmd.ExecuteNonQuery();
                        }

                        string insertAuditQuery = @"INSERT INTO dbo.ServiceRequestStatusAudit
                                                    (RequestID, PreviousStatus, NewStatus, ChangedBy, ChangedAtUtc)
                                                    VALUES (@RequestID, @PreviousStatus, @NewStatus, @ChangedBy, @ChangedAtUtc)";

                        using (SqlCommand auditCmd = new SqlCommand(insertAuditQuery, conn, tx))
                        {
                            auditCmd.Parameters.Add("@RequestID", System.Data.SqlDbType.Int).Value = requestId;
                            auditCmd.Parameters.Add("@PreviousStatus", System.Data.SqlDbType.NVarChar, 100).Value = expectedCurrentStatus;
                            auditCmd.Parameters.Add("@NewStatus", System.Data.SqlDbType.NVarChar, 100).Value = newStatus;
                            auditCmd.Parameters.Add("@ChangedBy", System.Data.SqlDbType.NVarChar, 100).Value = modifiedBy ?? "System";
                            auditCmd.Parameters.Add("@ChangedAtUtc", System.Data.SqlDbType.DateTime2).Value = changedAtUtc;
                            auditCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}