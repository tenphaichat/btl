using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QLCuaHangSach.DAL;

/// <summary>
/// Quản lý kết nối SQL Server và cung cấp các phương thức truy vấn dùng chung.
/// Mọi SqlException đều được dịch sang DataAccessException (thông báo thân thiện) và ghi log.
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

    /// <summary>Tạo tham số, tự đổi null thành DBNull.Value.</summary>
    public static SqlParameter Param(string name, object value) =>
        new SqlParameter(name, value ?? DBNull.Value);

    public static DataTable ExecuteQuery(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
    {
        try
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
        catch (SqlException ex)
        {
            throw SqlErrorTranslator.Dich(ex, "truy vấn: " + TomTat(sql));
        }
    }

    public static int ExecuteNonQuery(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
    {
        try
        {
            using var conn = CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn) { CommandType = type };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw SqlErrorTranslator.Dich(ex, "thực thi: " + TomTat(sql));
        }
    }

    public static object ExecuteScalar(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
    {
        try
        {
            using var conn = CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn) { CommandType = type };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw SqlErrorTranslator.Dich(ex, "đọc giá trị: " + TomTat(sql));
        }
    }

    private static string TomTat(string sql) =>
        sql.Length <= 60 ? sql.Replace("\r", " ").Replace("\n", " ") : sql.Substring(0, 60).Replace("\r", " ").Replace("\n", " ") + "...";
}
