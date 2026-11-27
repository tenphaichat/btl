using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Khách hàng - toàn bộ CRUD qua Stored Procedure (sp_KhachHang_*).</summary>
public class KhachHangDAL : IRepository<KhachHang>
{
    public DataTable GetAll() => GetAll("");

    /// <summary>Lấy danh sách, có thể lọc theo từ khóa (tên, SĐT, mã).</summary>
    public DataTable GetAll(string keyword) =>
        DatabaseConnection.ExecuteQuery("sp_KhachHang_GetAll", CommandType.StoredProcedure,
            DatabaseConnection.Param("@TuKhoa", keyword ?? ""));

    public KhachHang GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_KhachHang_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaKH", ma));
        if (dt.Rows.Count == 0) return null;
        DataRow r = dt.Rows[0];
        return new KhachHang
        {
            MaKH = r["MaKH"].ToString(),
            HoTen = r["HoTen"].ToString(),
            GioiTinh = Convert.ToBoolean(r["GioiTinh"]),
            NgaySinh = r["NgaySinh"] == DBNull.Value ? null : Convert.ToDateTime(r["NgaySinh"]),
            SoDienThoai = r["SoDienThoai"] as string,
            Email = r["Email"] as string,
            DiaChi = r["DiaChi"] as string,
            DiemTichLuy = Convert.ToInt32(r["DiemTichLuy"]),
            NgayDangKy = Convert.ToDateTime(r["NgayDangKy"])
        };
    }

    public bool Insert(KhachHang kh)
    {
        DatabaseConnection.ExecuteNonQuery("sp_KhachHang_Insert", CommandType.StoredProcedure, ThamSo(kh));
        return true;
    }

    public bool Update(KhachHang kh)
    {
        DatabaseConnection.ExecuteNonQuery("sp_KhachHang_Update", CommandType.StoredProcedure, ThamSo(kh));
        return true;
    }

    public bool Delete(string maKH)
    {
        DatabaseConnection.ExecuteNonQuery("sp_KhachHang_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaKH", maKH));
        return true;
    }

    public string GetLastCode() =>
        DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaKH FROM KhachHang ORDER BY MaKH DESC")?.ToString();

    private static SqlParameter[] ThamSo(KhachHang kh) => new[]
    {
        DatabaseConnection.Param("@MaKH", kh.MaKH),
        DatabaseConnection.Param("@HoTen", kh.HoTen),
        DatabaseConnection.Param("@GioiTinh", kh.GioiTinh),
        DatabaseConnection.Param("@NgaySinh", kh.NgaySinh),
        DatabaseConnection.Param("@SoDienThoai", kh.SoDienThoai),
        DatabaseConnection.Param("@Email", kh.Email),
        DatabaseConnection.Param("@DiaChi", kh.DiaChi),
        DatabaseConnection.Param("@DiemTichLuy", kh.DiemTichLuy)
    };
}
