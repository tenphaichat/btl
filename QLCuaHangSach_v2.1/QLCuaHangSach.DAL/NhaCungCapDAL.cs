using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Nhà cung cấp - toàn bộ CRUD qua Stored Procedure (sp_NhaCungCap_*).</summary>
public class NhaCungCapDAL : IRepository<NhaCungCap>
{
    public DataTable GetAll() =>
        DatabaseConnection.ExecuteQuery("sp_NhaCungCap_GetAll", CommandType.StoredProcedure);

    public NhaCungCap GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_NhaCungCap_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNCC", ma));
        if (dt.Rows.Count == 0) return null;
        DataRow r = dt.Rows[0];
        return new NhaCungCap
        {
            MaNCC = r["MaNCC"].ToString(),
            TenNCC = r["TenNCC"].ToString(),
            DiaChi = r["DiaChi"] as string,
            SoDienThoai = r["SoDienThoai"] as string,
            Email = r["Email"] as string
        };
    }

    public bool Insert(NhaCungCap e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhaCungCap_Insert", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Update(NhaCungCap e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhaCungCap_Update", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Delete(string ma)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhaCungCap_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNCC", ma));
        return true;
    }

    private static SqlParameter[] ThamSo(NhaCungCap e) => new[]
    {
        DatabaseConnection.Param("@MaNCC", e.MaNCC),
        DatabaseConnection.Param("@TenNCC", e.TenNCC),
        DatabaseConnection.Param("@DiaChi", e.DiaChi),
        DatabaseConnection.Param("@SoDienThoai", e.SoDienThoai),
        DatabaseConnection.Param("@Email", e.Email)
    };
}
