

namespace QLCuaHangSach.DTO;

/// <summary>
/// Lớp cơ sở trừu tượng cho mọi thực thể trong hệ thống.
/// Mỗi thực thể đều có một mã định danh duy nhất (khóa chính).
/// </summary>
public abstract class EntityBase : IValidatable
{
    /// <summary>Mã định danh (khóa chính) của thực thể.</summary>
    public abstract string Ma { get; }

    /// <summary>Chuỗi hiển thị thân thiện của thực thể (dùng cho ComboBox, thông báo...).</summary>
    public virtual string HienThi() => Ma;

    /// <summary>
    /// Trả về thông báo lỗi nếu dữ liệu sai, hoặc null nếu hợp lệ.
    /// Lớp con ghi đè (override) và gọi base.KiemTraDuLieu() trước để cộng dồn quy tắc.
    /// </summary>
    protected virtual string KiemTraDuLieu() =>
        string.IsNullOrWhiteSpace(Ma) ? "Mã không được để trống." : null;

    /// <summary>Thực thi hợp đồng IValidatable.</summary>
    public bool IsValid(out string message)
    {
        message = KiemTraDuLieu() ?? string.Empty;
        return message.Length == 0;
    }

    /// <summary>Phiên bản rút gọn: chỉ cần biết hợp lệ hay không.</summary>
    public bool HopLe() => IsValid(out _);

    public override string ToString() => HienThi();
}
