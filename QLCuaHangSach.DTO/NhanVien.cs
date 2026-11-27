

namespace QLCuaHangSach.DTO;

/// <summary>Nhân viên của cửa hàng - kế thừa từ ConNguoi.</summary>
public class NhanVien : ConNguoi
{
    public const string VAITRO_QUANLY = "QuanLy";
    public const string VAITRO_BANHANG = "BanHang";
    public const string VAITRO_THUKHO = "ThuKho";

    public string MaNV { get; set; }
    public string TenDangNhap { get; set; }
    public string MatKhauHash { get; set; }
    public string VaiTro { get; set; } = VAITRO_BANHANG;
    public bool TrangThai { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public override string Ma => MaNV;

    public string VaiTroText => LayTenVaiTro(VaiTro);

    public bool LaQuanLy => VaiTro == VAITRO_QUANLY;

    /// <summary>Chuyển mã vai trò sang tên hiển thị tiếng Việt.</summary>
    public static string LayTenVaiTro(string vaiTro) => vaiTro switch
    {
        VAITRO_QUANLY => "Quản lý",
        VAITRO_BANHANG => "Nhân viên bán hàng",
        VAITRO_THUKHO => "Thủ kho",
        _ => vaiTro
    };

    protected override string KiemTraDuLieu() =>
        base.KiemTraDuLieu() ?? (string.IsNullOrWhiteSpace(TenDangNhap) ? "Tên đăng nhập không được để trống." : null);
}
