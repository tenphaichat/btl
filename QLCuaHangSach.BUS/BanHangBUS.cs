using System.Data;
using System.Diagnostics;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>Nghiệp vụ bán hàng: kiểm tra tồn kho, tính giảm giá theo chính sách, lập hóa đơn trong một giao dịch.</summary>
public class BanHangBUS
{
    private readonly HoaDonDAL _dal = new HoaDonDAL();
    private readonly SachBUS _sachBus = new SachBUS();
    private readonly KhachHangBUS _khBus = new KhachHangBUS();

    /// <summary>Chính sách giảm giá đang áp dụng (có thể thay bằng cài đặt khác của IDiscountPolicy).</summary>
    public IDiscountPolicy ChinhSachGiamGia { get; set; } = new RankDiscountPolicy();

    public DataTable GetDanhSachHoaDon(DateTime? tu = null, DateTime? den = null) => _dal.GetAll(tu, den);
    public DataTable GetChiTietHoaDon(string maHD) => _dal.GetChiTiet(maHD);

    public string SinhMaHoaDonMoi() =>
        FormatHelper.GenerateNextCode("HD", _dal.GetLastCode(), 4);

    public KhachHang GetKhachHang(string maKH) => _khBus.GetById(maKH);

    /// <summary>Kiểm tra sách còn đủ để thêm vào giỏ (tính cả số lượng đã có trong giỏ).</summary>
    public void KiemTraTonTruocKhiThem(HoaDon hd, string maSach, int soLuongThem)
    {
        int daCo = hd.ChiTiet.FirstOrDefault(c => c.MaSach == maSach)?.SoLuong ?? 0;
        _sachBus.KiemTraTon(maSach, daCo + soLuongThem);
    }

    public decimal TyLeGiamGia(decimal tongTien, KhachHang kh) => ChinhSachGiamGia.TyLeGiamGia(tongTien, kh);

    public decimal TinhGiamGia(decimal tongTien, KhachHang kh) => ChinhSachGiamGia.TinhGiamGia(tongTien, kh);

    public bool ThanhToanHoaDon(HoaDon hd)
    {
        PhanQuyenBUS.YeuCau(ManHinh.BanHang);
        if (hd.ChiTiet.Count == 0)
            throw new BusinessException("Giỏ hàng đang trống. Vui lòng chọn ít nhất một đầu sách.");

        var sw = Stopwatch.StartNew();
        try
        {
            hd.MaNV = SessionManager.CurrentUser.MaNV;
            foreach (ChiTietHoaDon ct in hd.ChiTiet)
            {
                if (ct.SoLuong <= 0) throw new BusinessException($"Số lượng của sách \"{ct.TenSach}\" phải lớn hơn 0.");
                _sachBus.KiemTraTon(ct.MaSach, ct.SoLuong);
            }

            hd.TinhTongTien();
            hd.GiamGia = TinhGiamGia(hd.TongTien, GetKhachHang(hd.MaKH));   // tính lại ở BUS, không tin số liệu từ giao diện
            bool ok = _dal.CreateHoaDon(hd);

            sw.Stop();
            AuditBUS.Ghi("THANH_TOAN", hd.MaChungTu,
                         $"{hd.ChiTiet.Count} dòng, phải trả {hd.ThanhToan:N0} đ", sw.ElapsedMilliseconds);
            return ok;
        }
        catch (Exception ex) when (ex is BusinessException || ex is DataAccessException)
        {
            sw.Stop();
            AuditBUS.Ghi("THANH_TOAN", hd.MaChungTu, ex.Message, sw.ElapsedMilliseconds, false);
            throw;
        }
    }
}
