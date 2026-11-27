namespace QLCuaHangSach.Utils;

/// <summary>
/// Lỗi truy cập cơ sở dữ liệu đã được tầng DAL dịch sang thông điệp thân thiện.
/// Lỗi gốc của SQL Server nằm ở InnerException và đã được ghi vào file log.
/// </summary>
public class DataAccessException : Exception
{
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception inner) : base(message, inner) { }
}
