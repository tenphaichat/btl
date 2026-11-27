using System.Data;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập bảng AuditLog: ghi nhật ký người dùng, tra cứu nhật ký, thống kê hiệu năng nghiệp vụ.</summary>
public class AuditDAL
{
    public void Ghi(string maNV, string tenDangNhap, string hanhDong, string doiTuong, string chiTiet, int? thoiGianMs, bool thanhCong)
    {
        DatabaseConnection.ExecuteNonQuery("sp_AuditLog_Ghi", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNV", maNV),
            DatabaseConnection.Param("@TenDangNhap", tenDangNhap),
            DatabaseConnection.Param("@HanhDong", hanhDong),
            DatabaseConnection.Param("@DoiTuong", doiTuong),
            DatabaseConnection.Param("@ChiTiet", chiTiet),
            DatabaseConnection.Param("@ThoiGianMs", thoiGianMs),
            DatabaseConnection.Param("@ThanhCong", thanhCong));
    }

    public DataTable TimKiem(DateTime? tuNgay, DateTime? denNgay, string tenDangNhap, int top = 500) =>
        DatabaseConnection.ExecuteQuery("sp_AuditLog_TimKiem", CommandType.StoredProcedure,
            DatabaseConnection.Param("@TuNgay", tuNgay?.Date),
            DatabaseConnection.Param("@DenNgay", denNgay?.Date),
            DatabaseConnection.Param("@TenDangNhap", string.IsNullOrWhiteSpace(tenDangNhap) ? null : tenDangNhap.Trim()),
            DatabaseConnection.Param("@Top", top));

    public DataTable HieuNang() =>
        DatabaseConnection.ExecuteQuery("sp_AuditLog_HieuNang", CommandType.StoredProcedure);
}
