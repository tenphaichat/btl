namespace QLCuaHangSach.DTO
{
    /// <summary>Thể loại sách - kế thừa DanhMucBase (ThongTinThem = Mô tả).</summary>
    public class TheLoai : DanhMucBase
    {
        public string MoTa { get => ThongTinThem; set => ThongTinThem = value; }
    }

    /// <summary>Tác giả - kế thừa DanhMucBase (ThongTinThem = Quốc tịch).</summary>
    public class TacGia : DanhMucBase
    {
        public string QuocTich { get => ThongTinThem; set => ThongTinThem = value; }
    }

    /// <summary>Nhà xuất bản - kế thừa DanhMucBase (ThongTinThem = Địa chỉ).</summary>
    public class NhaXuatBan : DanhMucBase
    {
        public string DiaChi { get => ThongTinThem; set => ThongTinThem = value; }
    }

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
        public override bool HopLe() => base.HopLe() && !string.IsNullOrWhiteSpace(TenNCC);
    }

    /// <summary>Đầu sách được kinh doanh tại cửa hàng.</summary>
    public class Sach : EntityBase
    {
        public string MaSach { get; set; }
        public string TenSach { get; set; }
        public string MaTL { get; set; }
        public string TenTL { get; set; }
        public string MaTG { get; set; }
        public string TenTG { get; set; }
        public string MaNXB { get; set; }
        public string TenNXB { get; set; }
        public int? NamXuatBan { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
        public string MoTa { get; set; }
        public bool TrangThai { get; set; } = true;

        public override string Ma => MaSach;
        public override string HienThi() => $"{MaSach} - {TenSach}";

        /// <summary>Lợi nhuận dự kiến trên mỗi cuốn sách.</summary>
        public decimal LoiNhuanMoiCuon => GiaBan - GiaNhap;

        public bool ConHang(int soLuong = 1) => SoLuongTon >= soLuong;

        public override bool HopLe() =>
            base.HopLe() && !string.IsNullOrWhiteSpace(TenSach) && GiaBan >= 0 && GiaNhap >= 0;
    }
}
