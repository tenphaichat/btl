namespace QLCuaHangSach.DTO
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho mọi thực thể trong hệ thống.
    /// Mỗi thực thể đều có một mã định danh duy nhất (khóa chính).
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>Mã định danh (khóa chính) của thực thể.</summary>
        public abstract string Ma { get; }

        /// <summary>Chuỗi hiển thị thân thiện của thực thể (dùng cho ComboBox, thông báo...).</summary>
        public virtual string HienThi() => Ma;

        /// <summary>Kiểm tra dữ liệu tối thiểu của thực thể có hợp lệ hay không.</summary>
        public virtual bool HopLe() => !string.IsNullOrWhiteSpace(Ma);

        public override string ToString() => HienThi();
    }

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

        public override bool HopLe() => base.HopLe() && !string.IsNullOrWhiteSpace(HoTen);
    }

    /// <summary>
    /// Lớp trừu tượng cho các danh mục đơn giản gồm Mã - Tên - Thông tin thêm
    /// (lớp cha của TheLoai, TacGia, NhaXuatBan).
    /// </summary>
    public abstract class DanhMucBase : EntityBase
    {
        public string MaDM { get; set; }
        public string Ten { get; set; }
        public string ThongTinThem { get; set; }

        public override string Ma => MaDM;
        public override string HienThi() => Ten;
        public override bool HopLe() => base.HopLe() && !string.IsNullOrWhiteSpace(Ten);
    }
}
