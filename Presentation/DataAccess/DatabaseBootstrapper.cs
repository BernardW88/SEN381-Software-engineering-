using Microsoft.Data.SqlClient;
using System;
using System.Security.Cryptography;

namespace DataAccess
{
    public static class DatabaseBootstrapper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static void EnsureSchema()
        {
            using SqlConnection conn = DatabaseConnection.GetConnection();
            conn.Open();

            using SqlTransaction tx = conn.BeginTransaction();
            try
            {
                EnsureServiceRequestColumns(conn, tx);
                EnsureStatusAuditTable(conn, tx);
                EnsureUserCredentialColumns(conn, tx);
                BackfillLegacyUserCredentials(conn, tx);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static void EnsureServiceRequestColumns(SqlConnection conn, SqlTransaction tx)
        {
            const string sql = @"IF COL_LENGTH('dbo.ServiceRequests','UpdatedAt') IS NULL
BEGIN
    EXEC('ALTER TABLE dbo.ServiceRequests ADD UpdatedAt DATETIME2 NULL;');
END

EXEC('UPDATE dbo.ServiceRequests
      SET UpdatedAt = ISNULL(UpdatedAt, ISNULL(DateCreated, SYSUTCDATETIME()))
      WHERE UpdatedAt IS NULL;');

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceRequests')
      AND name = 'UpdatedAt'
      AND is_nullable = 1)
BEGIN
    EXEC('ALTER TABLE dbo.ServiceRequests ALTER COLUMN UpdatedAt DATETIME2 NOT NULL;');
END";

            using SqlCommand cmd = new SqlCommand(sql, conn, tx);
            cmd.ExecuteNonQuery();
        }

        private static void EnsureStatusAuditTable(SqlConnection conn, SqlTransaction tx)
        {
            const string sql = @"IF OBJECT_ID('dbo.ServiceRequestStatusAudit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ServiceRequestStatusAudit
    (
        AuditId INT IDENTITY(1,1) PRIMARY KEY,
        RequestID INT NOT NULL,
        PreviousStatus NVARCHAR(100) NOT NULL,
        NewStatus NVARCHAR(100) NOT NULL,
        ChangedBy NVARCHAR(100) NOT NULL,
        ChangedAtUtc DATETIME2 NOT NULL
    );
END";

            using SqlCommand cmd = new SqlCommand(sql, conn, tx);
            cmd.ExecuteNonQuery();
        }

        private static void EnsureUserCredentialColumns(SqlConnection conn, SqlTransaction tx)
        {
            const string sql = @"IF COL_LENGTH('dbo.Users','PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordHash NVARCHAR(256) NULL;
END

IF COL_LENGTH('dbo.Users','PasswordSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordSalt NVARCHAR(256) NULL;
END";

            using SqlCommand cmd = new SqlCommand(sql, conn, tx);
            cmd.ExecuteNonQuery();
        }

        private static void BackfillLegacyUserCredentials(SqlConnection conn, SqlTransaction tx)
        {
            const string selectSql = @"SELECT UserID, [Password]
FROM dbo.Users
WHERE ([PasswordHash] IS NULL OR [PasswordSalt] IS NULL)
  AND [Password] IS NOT NULL
  AND LTRIM(RTRIM([Password])) <> ''";

            using SqlCommand selectCmd = new SqlCommand(selectSql, conn, tx);
            using SqlDataReader reader = selectCmd.ExecuteReader();

            var updates = new System.Collections.Generic.List<(int UserId, string Hash, string Salt)>();
            while (reader.Read())
            {
                int userId = Convert.ToInt32(reader["UserID"]);
                string password = reader["Password"].ToString();
                (string hash, string salt) = HashPassword(password);
                updates.Add((userId, hash, salt));
            }
            reader.Close();

            const string updateSql = @"UPDATE dbo.Users
SET PasswordHash = @PasswordHash,
    PasswordSalt = @PasswordSalt
WHERE UserID = @UserID";

            foreach (var update in updates)
            {
                using SqlCommand updateCmd = new SqlCommand(updateSql, conn, tx);
                updateCmd.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 256).Value = update.Hash;
                updateCmd.Parameters.Add("@PasswordSalt", System.Data.SqlDbType.NVarChar, 256).Value = update.Salt;
                updateCmd.Parameters.Add("@UserID", System.Data.SqlDbType.Int).Value = update.UserId;
                updateCmd.ExecuteNonQuery();
            }
        }

        private static (string Hash, string Salt) HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }
    }
}
