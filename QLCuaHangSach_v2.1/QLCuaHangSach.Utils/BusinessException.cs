namespace QLCuaHangSach.Utils;

/// <summary>
/// Lỗi vi phạm quy tắc nghiệp vụ (ví dụ: bán vượt tồn kho, dữ liệu không hợp lệ).
/// Thông điệp được viết sẵn để hiển thị thẳng cho người dùng.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException() { }
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception inner) : base(message, inner) { }
}
