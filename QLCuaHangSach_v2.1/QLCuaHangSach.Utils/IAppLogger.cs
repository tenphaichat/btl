namespace QLCuaHangSach.Utils;

/// <summary>Hợp đồng ghi log: có thể thay FileLogger bằng bộ ghi log khác mà không sửa nơi sử dụng.</summary>
public interface IAppLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception ex = null);
}
