using System.Data;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Nhân viên - toàn bộ CRUD qua Stored Procedure (sp_NhanVien_*).</summary>
public class NhanVienDAL : IRepository<NhanVien>
{
    public DataTable GetAll() =>
        DatabaseConnection.ExecuteQuery("sp_NhanVien_GetAll", CommandType.StoredProcedure);

    public NhanVien GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_NhanVien_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNV", ma));
        return dt.Rows.Count == 0 ? null : MapRow(dt.Rows[0]);
    }

    public NhanVien GetByLogin(string username, string passwordHash)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_NhanVien_DangNhap", CommandType.StoredProcedure,
            DatabaseConnection.Param("@TenDangNhap", username),
            DatabaseConnection.Param("@MatKhauHash", passwordHash));
        return dt.Rows.Count == 0 ? null : MapRow(dt.Rows[0]);
    }

    // Các SP dùng THROW khi lỗi: không có ngoại lệ nghĩa là thành công (SET NOCOUNT ON nên không đếm dòng).
    public bool Insert(NhanVien nv)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhanVien_Insert", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNV", nv.MaNV),
            DatabaseConnection.Param("@HoTen", nv.HoTen),
            DatabaseConnection.Param("@GioiTinh", nv.GioiTinh),
            DatabaseConnection.Param("@NgaySinh", nv.NgaySinh),
            DatabaseConnection.Param("@SoDienThoai", nv.SoDienThoai),
            DatabaseConnection.Param("@Email", nv.Email),
            DatabaseConnection.Param("@DiaChi", nv.DiaChi),
            DatabaseConnection.Param("@TenDangNhap", nv.TenDangNhap),
            DatabaseConnection.Param("@MatKhauHash", nv.MatKhauHash),
            DatabaseConnection.Param("@VaiTro", nv.VaiTro),
            DatabaseConnection.Param("@TrangThai", nv.TrangThai));
        return true;
    }

    public bool Update(NhanVien nv)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhanVien_Update", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNV", nv.MaNV),
            DatabaseConnection.Param("@HoTen", nv.HoTen),
            DatabaseConnection.Param("@GioiTinh", nv.GioiTinh),
            DatabaseConnection.Param("@NgaySinh", nv.NgaySinh),
            DatabaseConnection.Param("@SoDienThoai", nv.SoDienThoai),
            DatabaseConnection.Param("@Email", nv.Email),
            DatabaseConnection.Param("@DiaChi", nv.DiaChi),
            DatabaseConnection.Param("@VaiTro", nv.VaiTro),
            DatabaseConnection.Param("@TrangThai", nv.TrangThai));
        return true;
    }

    /// <summary>"Xóa" nhân viên = khóa tài khoản để giữ lịch sử hóa đơn.</summary>
    public bool Delete(string maNV)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhanVien_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNV", maNV));
        return true;
    }

    public string GetLastCode() =>
        DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaNV FROM NhanVien ORDER BY MaNV DESC")?.ToString();

    private static NhanVien MapRow(DataRow r) => new NhanVien
    {
        MaNV = r["MaNV"].ToString(),
        HoTen = r["HoTen"].ToString(),
        GioiTinh = Convert.ToBoolean(r["GioiTinh"]),
        NgaySinh = r["NgaySinh"] == DBNull.Value ? null : Convert.ToDateTime(r["NgaySinh"]),
        SoDienThoai = r["SoDienThoai"] as string,
        Email = r["Email"] as string,
        DiaChi = r["DiaChi"] as string,
        TenDangNhap = r["TenDangNhap"].ToString(),
        MatKhauHash = r["MatKhauHash"].ToString(),
        VaiTro = r["VaiTro"].ToString(),
        TrangThai = Convert.ToBoolean(r["TrangThai"])
    };
}
