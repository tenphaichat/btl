using System.Data;
using System.Diagnostics;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>Nghiệp vụ nhập kho từ nhà cung cấp.</summary>
public class NhapHangBUS
{
    private readonly PhieuNhapDAL _dal = new PhieuNhapDAL();

    public DataTable GetDanhSachPhieuNhap() => _dal.GetAll();
    public DataTable GetChiTietPhieuNhap(string maPN) => _dal.GetChiTiet(maPN);

    public string SinhMaPhieuNhapMoi() =>
        FormatHelper.GenerateNextCode("PN", _dal.GetLastCode(), 4);

    public bool LuuPhieuNhap(PhieuNhap pn)
    {
        PhanQuyenBUS.YeuCau(ManHinh.NhapHang);
        if (string.IsNullOrWhiteSpace(pn.MaNCC))
            throw new BusinessException("Vui lòng chọn nhà cung cấp.");
        if (pn.ChiTiet.Count == 0)
            throw new BusinessException("Phiếu nhập phải có ít nhất một mặt hàng.");
        foreach (ChiTietPhieuNhap ct in pn.ChiTiet)
        {
            if (ct.SoLuong <= 0) throw new BusinessException($"Số lượng nhập của \"{ct.TenSach}\" phải lớn hơn 0.");
            if (ct.DonGia < 0) throw new BusinessException($"Đơn giá nhập của \"{ct.TenSach}\" không được âm.");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            pn.MaNV = SessionManager.CurrentUser.MaNV;
            pn.TinhTongTien();
            bool ok = _dal.CreatePhieuNhap(pn);

            sw.Stop();
            AuditBUS.Ghi("NHAP_KHO", pn.MaChungTu, $"{pn.ChiTiet.Count} dòng, tổng {pn.TongTien:N0} đ", sw.ElapsedMilliseconds);
            return ok;
        }
        catch (DataAccessException ex)
        {
            sw.Stop();
            AuditBUS.Ghi("NHAP_KHO", pn.MaChungTu, ex.Message, sw.ElapsedMilliseconds, false);
            throw;
        }
    }
}
