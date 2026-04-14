using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ScooterShare
{
    internal class DatabaseHelper
    {
        // Event raised after a new activity is logged (subscribers can refresh UI)
        public static event Action ActivityLogged;
        public static string ActivityLastError { get; private set; }
        private static string connectionString = ConfigurationManager.ConnectionStrings["ScooterDB"].ConnectionString;

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }

                }
            }
        }

        public static void ReseedIdentityToMax(string tableName, string idColumn)
        {
            try
            {
                var mx = ExecuteScalar($"SELECT ISNULL(MAX([{idColumn}]), 0) FROM [{tableName}]");
                int max = mx == null || mx == DBNull.Value ? 0 : Convert.ToInt32(mx);
                ExecuteNonQuery($"DBCC CHECKIDENT('{tableName}', RESEED, {max})");
            }
            catch { /* best-effort */ }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        // Ensure ActivityLog table exists (safe to call repeatedly)
        public static void EnsureActivityTableExists()
        {
            string q = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE [dbo].[ActivityLog] (
                                    id INT IDENTITY(1,1) PRIMARY KEY,
                                    message NVARCHAR(500) NOT NULL,
                                    activity_time DATETIME NOT NULL DEFAULT(GETDATE())
                                );
                            END";
            try { ExecuteNonQuery(q); } catch { /* silent */ }
        }

        public static bool LogActivity(string message)
        {
            try
            {
                ActivityLastError = null;
                EnsureActivityTableExists();
                // insert with explicit timestamp to avoid dependence on table default
                int rows = ExecuteNonQuery("INSERT INTO ActivityLog (message, activity_time) VALUES (@m, GETDATE())", new SqlParameter[] { new SqlParameter("@m", message) });
                if (rows > 0)
                {
                    try { ActivityLogged?.Invoke(); } catch { }
                    return true;
                }
                ActivityLastError = "No rows inserted into ActivityLog";
                System.Diagnostics.Debug.WriteLine(ActivityLastError);
                return false;
            }
            catch (Exception ex) { ActivityLastError = ex.Message; System.Diagnostics.Debug.WriteLine(ex); return false; }
        }

        // Try to find a likely date column for a table from common candidates
        public static string FindDateColumn(string tableName)
        {
            string[] candidates = new[] { "created_at", "created", "created_on", "createdDate", "registration_date", "register_date", "registered_at", "registered_on", "created_date", "createdAt" };
            string inList = string.Join(",", candidates.Select(c => "'" + c + "'"));
            string q = $"SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t AND COLUMN_NAME IN ({inList})";
            try
            {
                var res = ExecuteScalar(q, new SqlParameter[] { new SqlParameter("@t", tableName) });
                return res == null || res == DBNull.Value ? null : res.ToString();
            }
            catch { return null; }
        }
    }
}
