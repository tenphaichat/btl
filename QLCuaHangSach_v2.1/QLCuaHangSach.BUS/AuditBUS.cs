using System.Data;
using QLCuaHangSach.DAL;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>
/// Nhật ký người dùng và hiệu năng nghiệp vụ (bảng AuditLog).
/// Ghi nhật ký là việc phụ: nếu thất bại chỉ ghi cảnh báo vào file log, không làm hỏng nghiệp vụ chính.
/// </summary>
public class AuditBUS
{
    private static readonly AuditDAL Dal = new AuditDAL();

    /// <summary>Ghi một dòng nhật ký (người dùng lấy từ phiên đăng nhập hiện tại).</summary>
    /// <param name="thoiGianMs">Thời gian xử lý nghiệp vụ (ms); null nếu không đo.</param>
    /// <param name="tenDangNhapKhac">Dùng khi chưa đăng nhập (ví dụ đăng nhập sai).</param>
    public static void Ghi(string hanhDong, string doiTuong = null, string chiTiet = null,
                           long? thoiGianMs = null, bool thanhCong = true, string tenDangNhapKhac = null)
    {
        try
        {
            var user = SessionManager.CurrentUser;
            int? ms = thoiGianMs.HasValue ? (int)Math.Min(thoiGianMs.Value, int.MaxValue) : null;
            Dal.Ghi(user?.MaNV, user?.TenDangNhap ?? tenDangNhapKhac, hanhDong, doiTuong, chiTiet, ms, thanhCong);
        }
        catch (Exception ex)
        {
            AppLog.Warn($"Không ghi được nhật ký '{hanhDong}': {ex.Message}");
        }
    }

    public DataTable TimKiem(DateTime? tuNgay, DateTime? denNgay, string tenDangNhap)
    {
        PhanQuyenBUS.YeuCau(ManHinh.QuanTri);
        return Dal.TimKiem(tuNgay, denNgay, tenDangNhap);
    }

    public DataTable HieuNang()
    {
        PhanQuyenBUS.YeuCau(ManHinh.QuanTri);
        return Dal.HieuNang();
    }

    /// <summary>Đọc nội dung file log lỗi của một ngày (chỉ Quản lý).</summary>
    public string DocFileLog(DateTime ngay)
    {
        PhanQuyenBUS.YeuCau(ManHinh.QuanTri);
        if (AppLog.Instance is not FileLogger fl) return "(Bộ ghi log hiện tại không phải file.)";
        string path = fl.DuongDanLog(ngay);
        return File.Exists(path) ? File.ReadAllText(path) : "(Không có log nào trong ngày " + ngay.ToString("dd/MM/yyyy") + ".)";
    }

    public string ThuMucLog => (AppLog.Instance as FileLogger)?.ThuMucLog ?? "";
}
