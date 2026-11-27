using System.Data;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>Nghiệp vụ quản lý khách hàng thành viên.</summary>
public class KhachHangBUS
{
    private readonly KhachHangDAL _dal = new KhachHangDAL();

    public DataTable GetAll(string keyword = "") => _dal.GetAll(keyword);

    public KhachHang GetById(string maKH) =>
        string.IsNullOrWhiteSpace(maKH) ? null : _dal.GetById(maKH);

    public string SinhMaKHMoi() =>
        FormatHelper.GenerateNextCode("KH", _dal.GetLastCode(), 3);

    public bool LuuKhachHang(KhachHang kh, bool isNew)
    {
        PhanQuyenBUS.YeuCau(ManHinh.KhachHang);
        if (!kh.IsValid(out string loi)) throw new BusinessException(loi);
        if (!ValidationHelper.IsValidPhone(kh.SoDienThoai))
            throw new BusinessException("Số điện thoại không hợp lệ (10-11 số, bắt đầu bằng 0).");
        if (!ValidationHelper.IsValidEmail(kh.Email))
            throw new BusinessException("Địa chỉ Email không đúng định dạng.");

        bool ok = isNew ? _dal.Insert(kh) : _dal.Update(kh);
        AuditBUS.Ghi(isNew ? "THEM_KHACH_HANG" : "SUA_KHACH_HANG", kh.MaKH, kh.HoTen);
        return ok;
    }

    public bool XoaKhachHang(string maKH)
    {
        PhanQuyenBUS.YeuCau(ManHinh.KhachHang);
        bool ok = _dal.Delete(maKH);
        AuditBUS.Ghi("XOA_KHACH_HANG", maKH);
        return ok;
    }
}
