using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QLCuaHangSach.DAL
{
    /// <summary>
    /// Quản lý kết nối SQL Server và cung cấp các phương thức truy vấn dùng chung.
    /// </summary>
    public static class DatabaseConnection
    {
        private static readonly string DefaultConnStr =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QLCuaHangSach;Integrated Security=True;TrustServerCertificate=True";

        public static string ConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["QLCuaHangSach"];
                return setting != null && !string.IsNullOrWhiteSpace(setting.ConnectionString)
                    ? setting.ConnectionString
                    : DefaultConnStr;
            }
        }

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);

        public static DataTable ExecuteQuery(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
        {
            using var conn = CreateConnection();
            using var cmd = new SqlCommand(sql, conn) { CommandType = type };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public static int ExecuteNonQuery(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn) { CommandType = type };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn) { CommandType = type };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }
    }
}
