namespace QLCuaHangSach.DTO
{
    /// <summary>Lớp cơ sở cho một dòng chi tiết chứng từ (hóa đơn / phiếu nhập).</summary>
    public abstract class ChiTietBase
    {
        public string MaSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class ChiTietHoaDon : ChiTietBase { }

    public class ChiTietPhieuNhap : ChiTietBase { }

    /// <summary>
    /// Lớp chứng từ tổng quát (generic) - lớp cha của HoaDon và PhieuNhap.
    /// Một chứng từ gồm phần đầu (mã, ngày, nhân viên) và danh sách dòng chi tiết.
    /// </summary>
    public abstract class ChungTuBase<TChiTiet> : EntityBase where TChiTiet : ChiTietBase
    {
        public string MaChungTu { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string MaNV { get; set; }
        public string TenNV { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }
        public List<TChiTiet> ChiTiet { get; } = new List<TChiTiet>();

        public override string Ma => MaChungTu;

        /// <summary>Tên loại chứng từ (Hóa đơn bán hàng, Phiếu nhập kho...).</summary>
        public abstract string LoaiChungTu { get; }

        /// <summary>Tính lại tổng tiền dựa trên danh sách chi tiết.</summary>
        public virtual decimal TinhTongTien()
        {
            TongTien = ChiTiet.Sum(c => c.ThanhTien);
            return TongTien;
        }

        public int TongSoLuong => ChiTiet.Sum(c => c.SoLuong);

        /// <summary>Thêm sách vào chứng từ; nếu đã có thì cộng dồn số lượng.</summary>
        public void ThemChiTiet(TChiTiet ct)
        {
            TChiTiet daCo = ChiTiet.FirstOrDefault(x => x.MaSach == ct.MaSach);
            if (daCo != null) daCo.SoLuong += ct.SoLuong;
            else ChiTiet.Add(ct);
            TinhTongTien();
        }

        public void XoaChiTiet(string maSach)
        {
            ChiTiet.RemoveAll(x => x.MaSach == maSach);
            TinhTongTien();
        }

        public override string HienThi() => $"{LoaiChungTu} {MaChungTu} ({NgayLap:dd/MM/yyyy})";
    }

    /// <summary>Hóa đơn bán hàng.</summary>
    public class HoaDon : ChungTuBase<ChiTietHoaDon>
    {
        public string MaKH { get; set; }
        public string TenKH { get; set; }
        public decimal GiamGia { get; set; }
        public decimal ThanhToan => TongTien - GiamGia;
        public decimal TienKhachDua { get; set; }
        public decimal TienThoi => TienKhachDua - ThanhToan;

        public override string LoaiChungTu => "Hóa đơn";
    }

    /// <summary>Phiếu nhập hàng từ nhà cung cấp.</summary>
    public class PhieuNhap : ChungTuBase<ChiTietPhieuNhap>
    {
        public string MaNCC { get; set; }
        public string TenNCC { get; set; }

        public override string LoaiChungTu => "Phiếu nhập";
    }
}
