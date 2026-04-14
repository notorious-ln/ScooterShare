using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WpfMaterialControls.ViewModels
{
    internal static class DatabaseHelper
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["ScooterDB"].ConnectionString;

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static void ReseedIdentityToMax(string tableName, string idColumn)
        {
            try
            {
                object maxValue = ExecuteScalar($"SELECT ISNULL(MAX([{idColumn}]), 0) FROM [{tableName}]");
                int maxId = maxValue == null || maxValue == DBNull.Value ? 0 : Convert.ToInt32(maxValue);
                ExecuteNonQuery($"DBCC CHECKIDENT('{tableName}', RESEED, {maxId})");
            }
            catch
            {
                // best effort: if reseed fails, do not break the UI flow
            }
        }

        public static void EnsureActivityTableExists()
        {
            const string query = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ActivityLog]
    (
        id INT IDENTITY(1,1) PRIMARY KEY,
        message NVARCHAR(500) NOT NULL,
        activity_time DATETIME NOT NULL DEFAULT(GETDATE())
    );
END";

            ExecuteNonQuery(query);
        }

        public static void EnsureScooterSchema()
        {
            const string addOperationalStatus = @"
IF COL_LENGTH('Scooters', 'operational_status') IS NULL
BEGIN
    ALTER TABLE Scooters
    ADD operational_status NVARCHAR(50) NOT NULL
        CONSTRAINT DF_Scooters_OperationalStatus DEFAULT(N'Доступен');
END";

            const string addBatteryPercent = @"
IF COL_LENGTH('Scooters', 'battery_percent') IS NULL
BEGIN
    ALTER TABLE Scooters
    ADD battery_percent INT NOT NULL
        CONSTRAINT DF_Scooters_BatteryPercent DEFAULT(100);
END";

            const string addCurrentLocation = @"
IF COL_LENGTH('Scooters', 'current_location') IS NULL
BEGIN
    ALTER TABLE Scooters
    ADD current_location NVARCHAR(150) NOT NULL
        CONSTRAINT DF_Scooters_CurrentLocation DEFAULT(N'Не указано');
END";

            const string normalizeOperationalStatus = @"
UPDATE Scooters
SET operational_status = N'Доступен'
WHERE operational_status IS NULL OR LTRIM(RTRIM(operational_status)) = N'';";

            const string normalizeBatteryPercent = @"
UPDATE Scooters
SET battery_percent = 100
WHERE battery_percent IS NULL OR battery_percent < 0 OR battery_percent > 100;";

            const string normalizeCurrentLocation = @"
UPDATE Scooters
SET current_location = N'Не указано'
WHERE current_location IS NULL OR LTRIM(RTRIM(current_location)) = N'';";

            const string addBatteryConstraint = @"
IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE [name] = 'CK_Scooters_BatteryPercent'
)
BEGIN
    ALTER TABLE Scooters
    ADD CONSTRAINT CK_Scooters_BatteryPercent
    CHECK (battery_percent >= 0 AND battery_percent <= 100);
END";

            ExecuteNonQuery(addOperationalStatus);
            ExecuteNonQuery(addBatteryPercent);
            ExecuteNonQuery(addCurrentLocation);
            ExecuteNonQuery(normalizeOperationalStatus);
            ExecuteNonQuery(normalizeBatteryPercent);
            ExecuteNonQuery(normalizeCurrentLocation);
            ExecuteNonQuery(addBatteryConstraint);
        }

        public static bool LogActivity(string message)
        {
            try
            {
                EnsureActivityTableExists();
                int rows = ExecuteNonQuery(
                    "INSERT INTO ActivityLog (message, activity_time) VALUES (@m, GETDATE())",
                    new[] { new SqlParameter("@m", message) });
                return rows > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
