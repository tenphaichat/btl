

namespace QLCuaHangSach.DTO;

/// <summary>
/// Lớp trừu tượng mô tả thông tin chung của một con người
/// (dùng làm lớp cha cho NhanVien và KhachHang).
/// </summary>
public abstract class ConNguoi : EntityBase
{
    public string HoTen { get; set; }
    public bool GioiTinh { get; set; } = true;
    public DateTime? NgaySinh { get; set; }
    public string SoDienThoai { get; set; }
    public string Email { get; set; }
    public string DiaChi { get; set; }

    public string GioiTinhText => GioiTinh ? "Nam" : "Nữ";

    /// <summary>Tính tuổi hiện tại dựa vào ngày sinh.</summary>
    public virtual int? TinhTuoi()
    {
        if (NgaySinh == null) return null;
        DateTime today = DateTime.Today;
        int tuoi = today.Year - NgaySinh.Value.Year;
        if (NgaySinh.Value.Date > today.AddYears(-tuoi)) tuoi--;
        return tuoi;
    }

    public override string HienThi() => $"{Ma} - {HoTen}";

    protected override string KiemTraDuLieu() =>
        base.KiemTraDuLieu() ?? (string.IsNullOrWhiteSpace(HoTen) ? "Họ tên không được để trống." : null);
}
