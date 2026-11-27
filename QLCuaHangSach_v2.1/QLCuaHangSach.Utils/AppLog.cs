namespace QLCuaHangSach.Utils;

/// <summary>Điểm truy cập log dùng chung toàn ứng dụng (mặc định ghi file; có thể thay bộ ghi qua Instance).</summary>
public static class AppLog
{
    public static IAppLogger Instance { get; set; } = new FileLogger();

    public static void Info(string message) => Instance.Info(message);
    public static void Warn(string message) => Instance.Warn(message);
    public static void Error(string message, Exception ex = null) => Instance.Error(message, ex);
}
