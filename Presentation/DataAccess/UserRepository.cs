using Microsoft.Data.SqlClient;
using Models;
using System;

namespace DataAccess
{
    public class UserRepository : IUserRepository
    {
        public UserAuthRecord GetUserAuthRecord(string username)
        {
            using (SqlConnection conn = DatabaseConnection.GetConnection())
            {
                string query = @"SELECT UserID, Username, Role,
                                        CASE WHEN COL_LENGTH('dbo.Users','Password') IS NULL THEN NULL ELSE [Password] END AS LegacyPassword,
                                        CASE WHEN COL_LENGTH('dbo.Users','PasswordHash') IS NULL THEN NULL ELSE [PasswordHash] END AS PasswordHash,
                                        CASE WHEN COL_LENGTH('dbo.Users','PasswordSalt') IS NULL THEN NULL ELSE [PasswordSalt] END AS PasswordSalt
                                 FROM Users
                                 WHERE Username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 100).Value = username;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserAuthRecord
                            {
                                User = new User
                                {
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    Username = reader["Username"].ToString(),
                                    Role = reader["Role"].ToString()
                                },
                                LegacyPassword = reader["LegacyPassword"] == DBNull.Value ? null : reader["LegacyPassword"].ToString(),
                                PasswordHash = reader["PasswordHash"] == DBNull.Value ? null : reader["PasswordHash"].ToString(),
                                PasswordSalt = reader["PasswordSalt"] == DBNull.Value ? null : reader["PasswordSalt"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}