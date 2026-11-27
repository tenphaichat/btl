using System.Windows.Forms;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>
/// Xử lý lỗi tập trung ở giao diện (Lab 15): phân biệt lỗi nghiệp vụ, lỗi cơ sở dữ liệu và lỗi không xác định.
/// Người dùng chỉ thấy thông báo thân thiện; chi tiết kỹ thuật nằm trong file log.
/// </summary>
public static class ErrorHandler
{
    public static void Show(IWin32Window owner, Exception ex, string tieuDe = null)
    {
        switch (ex)
        {
            case BusinessException:
                MessageBox.Show(owner, ex.Message, tieuDe ?? "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;
            case DataAccessException:   // đã được ghi log chi tiết ở tầng DAL
                MessageBox.Show(owner, ex.Message, tieuDe ?? "Lỗi cơ sở dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
            default:
                AppLog.Error("Lỗi không xác định", ex);
                MessageBox.Show(owner, "Ứng dụng gặp lỗi không mong muốn. Chi tiết đã được ghi vào file log.",
                    tieuDe ?? "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
        }
    }

    /// <summary>Thông báo ngắn gọn để hiển thị trên nhãn/thanh trạng thái (không hiện stack trace, không lộ chuỗi kết nối).</summary>
    public static string Message(Exception ex)
    {
        if (ex is BusinessException || ex is DataAccessException) return ex.Message;
        AppLog.Error("Lỗi không xác định", ex);
        return "Lỗi không mong muốn (chi tiết trong file log).";
    }
}
