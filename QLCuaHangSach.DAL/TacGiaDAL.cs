using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Truy cập dữ liệu Tác giả - toàn bộ CRUD qua Stored Procedure (sp_TacGia_*).</summary>
public class TacGiaDAL : IRepository<TacGia>
{
    public DataTable GetAll() =>
        DatabaseConnection.ExecuteQuery("sp_TacGia_GetAll", CommandType.StoredProcedure);

    public TacGia GetById(string ma)
    {
        DataTable dt = DatabaseConnection.ExecuteQuery("sp_TacGia_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaTG", ma));
        if (dt.Rows.Count == 0) return null;
        DataRow r = dt.Rows[0];
        return new TacGia
        {
            MaDM = r["MaTG"].ToString(),
            Ten = r["TenTG"].ToString(),
            QuocTich = r["QuocTich"] as string
        };
    }

    public bool Insert(TacGia e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_TacGia_Insert", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Update(TacGia e)
    {
        DatabaseConnection.ExecuteNonQuery("sp_TacGia_Update", CommandType.StoredProcedure, ThamSo(e));
        return true;
    }

    public bool Delete(string ma)
    {
        DatabaseConnection.ExecuteNonQuery("sp_TacGia_Delete", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaTG", ma));
        return true;
    }

    private static SqlParameter[] ThamSo(TacGia e) => new[]
    {
        DatabaseConnection.Param("@MaTG", e.MaDM),
        DatabaseConnection.Param("@TenTG", e.Ten),
        DatabaseConnection.Param("@QuocTich", e.QuocTich)
    };
}
