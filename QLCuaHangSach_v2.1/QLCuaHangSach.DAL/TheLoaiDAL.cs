using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Thể loại - toàn bộ CRUD qua Stored Procedure (sp_TheLoai_*).</summary>
public class TheLoaiDAL : IRepository<TheLoai>
{
    public DataTable GetAll() =>
        DatabaseConnection.ExecuteQuery("sp_TheLoai_GetAll", CommandType.StoredProcedure);

    public TheLoai GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_TheLoai_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaTL", ma));
        if (dt.Rows.Count == 0) return null;
        DataRow r = dt.Rows[0];
        return new TheLoai
        {
            MaDM = r["MaTL"].ToString(),
            Ten = r["TenTL"].ToString(),
            MoTa = r["MoTa"] as string
        };
    }

    public bool Insert(TheLoai e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_TheLoai_Insert", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Update(TheLoai e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_TheLoai_Update", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Delete(string ma)
    {
        DatabaseConnection.ExecuteNonQuery("sp_TheLoai_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaTL", ma));
        return true;
    }

    private static SqlParameter[] ThamSo(TheLoai e) => new[]
    {
        DatabaseConnection.Param("@MaTL", e.MaDM),
        DatabaseConnection.Param("@TenTL", e.Ten),
        DatabaseConnection.Param("@MoTa", e.MoTa)
    };
}
