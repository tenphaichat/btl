using System.Text;

namespace QLCuaHangSach.Utils;

/// <summary>
/// Ghi log vào tệp văn bản theo ngày: Logs\yyyyMMdd.log (cạnh file chạy).
/// Tự tạo thư mục, tự xóa file log cũ hơn số ngày quy định và không bao giờ làm ứng dụng lỗi thêm khi ghi log thất bại.
/// </summary>
public class FileLogger : IAppLogger
{
    private static readonly object Sync = new object();
    private readonly int _soNgayGiuLog;

    public string ThuMucLog { get; }

    public FileLogger(string thuMuc = null, int soNgayGiuLog = 30)
    {
        _soNgayGiuLog = soNgayGiuLog;
        ThuMucLog = thuMuc ?? ChonThuMucGhiDuoc();
        DonLogCu();
    }

    /// <summary>Đường dẫn file log của một ngày (mặc định hôm nay).</summary>
    public string DuongDanLog(DateTime? ngay = null) =>
        Path.Combine(ThuMucLog, (ngay ?? DateTime.Now).ToString("yyyyMMdd") + ".log");

    public void Info(string message) => Ghi("INFO ", message, null);
    public void Warn(string message) => Ghi("WARN ", message, null);
    public void Error(string message, Exception ex = null) => Ghi("ERROR", message, ex);

    private void Ghi(string mucDo, string message, Exception ex)
    {
        try
        {
            Directory.CreateDirectory(ThuMucLog);
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(" | ").Append(mucDo).Append(" | ").AppendLine(message);
            if (ex != null) sb.AppendLine(ex.ToString());
            lock (Sync) File.AppendAllText(DuongDanLog(), sb.ToString(), Encoding.UTF8);
        }
        catch
        {
            // Lỗi khi ghi log không được phép làm dừng ứng dụng.
        }
    }

    /// <summary>Ưu tiên thư mục Logs cạnh file chạy; nếu không có quyền ghi thì dùng %LocalAppData%.</summary>
    private static string ChonThuMucGhiDuoc()
    {
        string canhExe = Path.Combine(AppContext.BaseDirectory, "Logs");
        try
        {
            Directory.CreateDirectory(canhExe);
            string thu = Path.Combine(canhExe, ".write_test");
            File.WriteAllText(thu, "ok");
            File.Delete(thu);
            return canhExe;
        }
        catch
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QLCuaHangSach", "Logs");
        }
    }

    private void DonLogCu()
    {
        try
        {
            if (!Directory.Exists(ThuMucLog)) return;
            DateTime han = DateTime.Now.AddDays(-_soNgayGiuLog);
            foreach (string f in Directory.GetFiles(ThuMucLog, "*.log"))
                if (File.GetLastWriteTime(f) < han) File.Delete(f);
        }
        catch
        {
            // Bỏ qua: dọn log là việc phụ.
        }
    }
}
