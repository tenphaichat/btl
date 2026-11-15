namespace QLCuaHangSach.DTO
{
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

        public override bool HopLe() => base.HopLe() && !string.IsNullOrWhiteSpace(TenDangNhap);
    }

    /// <summary>Khách hàng thành viên - kế thừa từ ConNguoi.</summary>
    public class KhachHang : ConNguoi
    {
        public string MaKH { get; set; }
        public int DiemTichLuy { get; set; }
        public DateTime NgayDangKy { get; set; } = DateTime.Today;

        public override string Ma => MaKH;

        /// <summary>Hạng thành viên xác định theo điểm tích lũy.</summary>
        public string HangThanhVien => DiemTichLuy switch
        {
            >= 300 => "Kim cương",
            >= 150 => "Vàng",
            >= 50 => "Bạc",
            _ => "Đồng"
        };

        /// <summary>Tỷ lệ giảm giá được hưởng theo hạng thành viên.</summary>
        public decimal TyLeGiamGia() => DiemTichLuy switch
        {
            >= 300 => 0.10m,
            >= 150 => 0.07m,
            >= 50 => 0.05m,
            _ => 0.02m
        };

        public override string HienThi() => $"{HoTen} - {SoDienThoai}";
    }
}
