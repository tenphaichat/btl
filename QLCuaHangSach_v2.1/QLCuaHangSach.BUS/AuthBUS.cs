using System.Data;
using System.Diagnostics;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>Đăng nhập và quản lý tài khoản nhân viên.</summary>
public class AuthBUS
{
    private readonly NhanVienDAL _dal = new NhanVienDAL();

    public NhanVien Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new BusinessException("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");

        var sw = Stopwatch.StartNew();
        string hash = SecurityHelper.HashPasswordSHA256(password.Trim());
        NhanVien nv = _dal.GetByLogin(username.Trim(), hash);
        sw.Stop();

        if (nv == null)
        {
            AuditBUS.Ghi("DANG_NHAP", null, "Sai tên đăng nhập/mật khẩu hoặc tài khoản đã khóa", sw.ElapsedMilliseconds,
                         false, username.Trim());
            throw new BusinessException("Tên đăng nhập hoặc mật khẩu không chính xác, hoặc tài khoản đã khóa.");
        }

        SessionManager.Login(nv);
        AuditBUS.Ghi("DANG_NHAP", nv.MaNV, "Vai trò: " + nv.VaiTroText, sw.ElapsedMilliseconds);
        return nv;
    }

    public void Logout()
    {
        AuditBUS.Ghi("DANG_XUAT", SessionManager.CurrentUser?.MaNV);
        SessionManager.Logout();
    }

    public DataTable GetAllNhanVien()
    {
        PhanQuyenBUS.YeuCau(ManHinh.NhanVien);
        return _dal.GetAll();
    }

    public string SinhMaNhanVienMoi()
    {
        PhanQuyenBUS.YeuCau(ManHinh.NhanVien);
        return FormatHelper.GenerateNextCode("NV", _dal.GetLastCode(), 2);
    }

    public bool SaveNhanVien(NhanVien nv, string rawPassword, bool isNew)
    {
        PhanQuyenBUS.YeuCau(ManHinh.NhanVien);
        if (!nv.IsValid(out string loi)) throw new BusinessException(loi);
        if (!string.IsNullOrWhiteSpace(nv.SoDienThoai) && !ValidationHelper.IsValidPhone(nv.SoDienThoai))
            throw new BusinessException("Số điện thoại nhân viên không hợp lệ (10-11 chữ số, bắt đầu bằng 0).");

        bool ok;
        if (isNew)
        {
            nv.MatKhauHash = SecurityHelper.HashPasswordSHA256(string.IsNullOrWhiteSpace(rawPassword) ? "123456" : rawPassword);
            ok = _dal.Insert(nv);
        }
        else
        {
            ok = _dal.Update(nv);
        }
        AuditBUS.Ghi(isNew ? "THEM_NHAN_VIEN" : "SUA_NHAN_VIEN", nv.MaNV, nv.HoTen + " - " + nv.VaiTroText);
        return ok;
    }

    public bool LockNhanVien(string maNV)
    {
        PhanQuyenBUS.YeuCau(ManHinh.NhanVien);
        if (SessionManager.CurrentUser?.MaNV == maNV)
            throw new BusinessException("Không thể tự khóa tài khoản đang đăng nhập.");
        bool ok = _dal.Delete(maNV);
        AuditBUS.Ghi("KHOA_NHAN_VIEN", maNV);
        return ok;
    }
}
