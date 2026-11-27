using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Nhà xuất bản - toàn bộ CRUD qua Stored Procedure (sp_NhaXuatBan_*).</summary>
public class NhaXuatBanDAL : IRepository<NhaXuatBan>
{
    public DataTable GetAll() =>
        DatabaseConnection.ExecuteQuery("sp_NhaXuatBan_GetAll", CommandType.StoredProcedure);

    public NhaXuatBan GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_NhaXuatBan_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNXB", ma));
        if (dt.Rows.Count == 0) return null;
        DataRow r = dt.Rows[0];
        return new NhaXuatBan
        {
            MaDM = r["MaNXB"].ToString(),
            Ten = r["TenNXB"].ToString(),
            DiaChi = r["DiaChi"] as string
        };
    }

    public bool Insert(NhaXuatBan e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhaXuatBan_Insert", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Update(NhaXuatBan e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhaXuatBan_Update", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Delete(string ma)
    {
        DatabaseConnection.ExecuteNonQuery("sp_NhaXuatBan_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaNXB", ma));
        return true;
    }

    private static SqlParameter[] ThamSo(NhaXuatBan e) => new[]
    {
        DatabaseConnection.Param("@MaNXB", e.MaDM),
        DatabaseConnection.Param("@TenNXB", e.Ten),
        DatabaseConnection.Param("@DiaChi", e.DiaChi)
    };
}
