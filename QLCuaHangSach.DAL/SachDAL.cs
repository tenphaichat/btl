using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Sách - toàn bộ CRUD qua Stored Procedure (sp_Sach_*, sp_TimKiemSach).</summary>
public class SachDAL : IRepository<Sach>
{
    public DataTable GetAll() => Search("", null);

    public DataTable Search(string keyword = "", string maTL = null) =>
        DatabaseConnection.ExecuteQuery("sp_TimKiemSach", CommandType.StoredProcedure,
            DatabaseConnection.Param("@TuKhoa", keyword ?? ""),
            DatabaseConnection.Param("@MaTL", string.IsNullOrWhiteSpace(maTL) ? null : maTL));

    /// <summary>
    /// Tìm kiếm đa tiêu chí có sắp xếp và phân trang (OFFSET-FETCH).
    /// Dùng SqlDataReader nạp vào DataTable (Lab 9) và tham số OUTPUT @TongSo (Lab 8) để biết tổng số dòng.
    /// </summary>
    public DataTable SearchPaged(string keyword, string maTL, decimal? giaTu, decimal? giaDen,
                                 string sapXep, bool giamDan, int trang, int soDong, out int tongSo)
    {
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand("sp_Sach_PhanTrang", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(DatabaseConnection.Param("@TuKhoa", keyword ?? ""));
            cmd.Parameters.Add(DatabaseConnection.Param("@MaTL", string.IsNullOrWhiteSpace(maTL) ? null : maTL));
            cmd.Parameters.Add(DatabaseConnection.Param("@GiaTu", giaTu));
            cmd.Parameters.Add(DatabaseConnection.Param("@GiaDen", giaDen));
            cmd.Parameters.Add(DatabaseConnection.Param("@SapXep", sapXep));
            cmd.Parameters.Add(DatabaseConnection.Param("@GiamDan", giamDan));
            cmd.Parameters.Add(DatabaseConnection.Param("@Offset", (trang - 1) * soDong));
            cmd.Parameters.Add(DatabaseConnection.Param("@PageSize", soDong));
            var pTongSo = new SqlParameter("@TongSo", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pTongSo);

            conn.Open();
            var table = new DataTable("Sach");
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                table.Load(reader);
            }

            // Giá trị OUTPUT chỉ có sau khi reader đã đọc xong và được đóng
            tongSo = pTongSo.Value == DBNull.Value ? 0 : Convert.ToInt32(pTongSo.Value);
            return table;
        }
        catch (SqlException ex)
        {
            throw SqlErrorTranslator.Dich(ex, "tìm kiếm sách (phân trang)");
        }
    }

    public Sach GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_Sach_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaSach", ma));
        if (dt.Rows.Count == 0) return null;
        DataRow r = dt.Rows[0];
        return new Sach
        {
            MaSach = r["MaSach"].ToString(),
            TenSach = r["TenSach"].ToString(),
            MaTL = r["MaTL"].ToString(),
            TenTL = r["TenTL"].ToString(),
            MaTG = r["MaTG"].ToString(),
            TenTG = r["TenTG"].ToString(),
            MaNXB = r["MaNXB"].ToString(),
            TenNXB = r["TenNXB"].ToString(),
            NamXuatBan = r["NamXuatBan"] == DBNull.Value ? null : Convert.ToInt32(r["NamXuatBan"]),
            GiaNhap = Convert.ToDecimal(r["GiaNhap"]),
            GiaBan = Convert.ToDecimal(r["GiaBan"]),
            SoLuongTon = Convert.ToInt32(r["SoLuongTon"]),
            MoTa = r["MoTa"] as string,
            TrangThai = Convert.ToBoolean(r["TrangThai"])
        };
    }

    public bool Insert(Sach s)
    {
        DatabaseConnection.ExecuteNonQuery("sp_Sach_Insert", CommandType.StoredProcedure, ThamSo(s));
        return true;
    }

    public bool Update(Sach s)
    {
        DatabaseConnection.ExecuteNonQuery("sp_Sach_Update", CommandType.StoredProcedure, ThamSo(s));
        return true;
    }

    /// <summary>"Xóa" sách = ngừng kinh doanh để giữ chi tiết hóa đơn cũ.</summary>
    public bool Delete(string maSach)
    {
        DatabaseConnection.ExecuteNonQuery("sp_Sach_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaSach", maSach));
        return true;
    }

    public string GetLastCode() =>
        DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaSach FROM Sach ORDER BY MaSach DESC")?.ToString();

    private static SqlParameter[] ThamSo(Sach s) => new[]
    {
        DatabaseConnection.Param("@MaSach", s.MaSach),
        DatabaseConnection.Param("@TenSach", s.TenSach),
        DatabaseConnection.Param("@MaTL", s.MaTL),
        DatabaseConnection.Param("@MaTG", s.MaTG),
        DatabaseConnection.Param("@MaNXB", s.MaNXB),
        DatabaseConnection.Param("@NamXuatBan", s.NamXuatBan),
        DatabaseConnection.Param("@GiaNhap", s.GiaNhap),
        DatabaseConnection.Param("@GiaBan", s.GiaBan),
        DatabaseConnection.Param("@SoLuongTon", s.SoLuongTon),
        DatabaseConnection.Param("@MoTa", s.MoTa)
    };
}
