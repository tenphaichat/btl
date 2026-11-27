using System.Data.SqlClient;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.DAL;

/// <summary>
/// Dịch lỗi gốc của SQL Server (SqlException) thành thông báo tiếng Việt dễ hiểu cho người dùng
/// và ghi chi tiết lỗi gốc vào file log để lập trình viên tra cứu.
/// </summary>
public static class SqlErrorTranslator
{
    public static DataAccessException Dich(SqlException ex, string nguCanh)
    {
        AppLog.Error($"Lỗi SQL #{ex.Number} khi {nguCanh}", ex);

        string thongBao = ex.Number switch
        {
            2627 or 2601 => "Dữ liệu bị trùng: mã hoặc giá trị duy nhất đã tồn tại.",
            547 when ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase)
                => "Không thể thực hiện vì dữ liệu đang được sử dụng ở nơi khác.",
            547 when ex.Message.Contains("CHECK", StringComparison.OrdinalIgnoreCase)
                => "Dữ liệu vi phạm ràng buộc kiểm tra (ví dụ: tồn kho không đủ hoặc giá trị âm).",
            547 => "Dữ liệu vi phạm ràng buộc khóa ngoại.",
            515 => "Thiếu dữ liệu bắt buộc.",
            8152 => "Dữ liệu nhập vào quá dài.",
            1205 => "Hệ thống đang bận, vui lòng thử lại sau ít phút.",
            2 or 53 or 40 or 4060 or 18456 or 1326 or -1
                => "Không kết nối được tới SQL Server. Hãy kiểm tra dịch vụ SQL Server và chuỗi kết nối trong App.config.",
            >= 50000 => ex.Message,   // lỗi nghiệp vụ do Stored Procedure THROW
            _ => "Lỗi cơ sở dữ liệu. Chi tiết đã được ghi vào file log."
        };
        return new DataAccessException(thongBao, ex);
    }
}
