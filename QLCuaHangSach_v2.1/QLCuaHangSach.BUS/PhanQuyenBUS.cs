using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>
/// Phân quyền theo vai trò. GUI dùng CoQuyen() để ẩn/khóa menu; các lớp BUS dùng YeuCau()
/// để chặn thao tác ghi ngay cả khi ai đó cố gọi tắt qua GUI.
/// </summary>
public static class PhanQuyenBUS
{
    private static readonly Dictionary<string, ManHinh[]> BangQuyen = new()
    {
        [NhanVien.VAITRO_QUANLY] = Enum.GetValues<ManHinh>(),
        [NhanVien.VAITRO_BANHANG] = new[] { ManHinh.Dashboard, ManHinh.BanHang, ManHinh.KhachHang, ManHinh.ThongKe },
        [NhanVien.VAITRO_THUKHO] = new[] { ManHinh.Dashboard, ManHinh.Sach, ManHinh.DanhMuc, ManHinh.NhapHang }
    };

    public static bool CoQuyen(string vaiTro, ManHinh manHinh) =>
        vaiTro != null && BangQuyen.TryGetValue(vaiTro, out ManHinh[] ds) && ds.Contains(manHinh);

    /// <summary>Người dùng đang đăng nhập có quyền với màn hình này không.</summary>
    public static bool CoQuyen(ManHinh manHinh) =>
        SessionManager.CurrentUser != null && CoQuyen(SessionManager.CurrentUser.VaiTro, manHinh);

    /// <summary>Ném BusinessException nếu người dùng hiện tại không có quyền; đồng thời ghi nhật ký từ chối.</summary>
    public static void YeuCau(ManHinh manHinh)
    {
        if (CoQuyen(manHinh)) return;

        string vaiTro = SessionManager.CurrentUser?.VaiTroText ?? "chưa đăng nhập";
        AuditBUS.Ghi("TU_CHOI_QUYEN", manHinh.ToString(), "Vai trò: " + vaiTro, null, false);
        throw new BusinessException($"Bạn ({vaiTro}) không có quyền thực hiện chức năng \"{TenManHinh(manHinh)}\".");
    }

    public static string TenManHinh(ManHinh manHinh) => manHinh switch
    {
        ManHinh.Dashboard => "Trang chủ",
        ManHinh.Sach => "Quản lý sách",
        ManHinh.DanhMuc => "Quản lý danh mục",
        ManHinh.BanHang => "Bán hàng",
        ManHinh.NhapHang => "Nhập kho",
        ManHinh.KhachHang => "Quản lý khách hàng",
        ManHinh.NhanVien => "Quản lý nhân viên",
        ManHinh.ThongKe => "Báo cáo & thống kê",
        ManHinh.QuanTri => "Quản trị hệ thống",
        _ => manHinh.ToString()
    };
}
