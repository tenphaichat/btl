

namespace QLCuaHangSach.DTO;

/// <summary>Nhà cung cấp sách cho cửa hàng.</summary>
public class NhaCungCap : EntityBase
{
    public string MaNCC { get; set; }
    public string TenNCC { get; set; }
    public string DiaChi { get; set; }
    public string SoDienThoai { get; set; }
    public string Email { get; set; }

    public override string Ma => MaNCC;
    public override string HienThi() => TenNCC;
    protected override string KiemTraDuLieu() =>
        base.KiemTraDuLieu() ?? (string.IsNullOrWhiteSpace(TenNCC) ? "Tên nhà cung cấp không được để trống." : null);
}
