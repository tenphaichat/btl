using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

public class PhieuNhapDAL
{
    public DataTable GetAll() =>
        DatabaseConnection.ExecuteQuery(
            @"SELECT pn.MaPN, pn.NgayNhap, nv.HoTen AS NhanVien, ncc.TenNCC AS NhaCungCap, pn.TongTien, pn.GhiChu
              FROM PhieuNhap pn
              JOIN NhanVien nv ON nv.MaNV = pn.MaNV
              JOIN NhaCungCap ncc ON ncc.MaNCC = pn.MaNCC
              ORDER BY pn.NgayNhap DESC");

    public DataTable GetChiTiet(string maPN) =>
        DatabaseConnection.ExecuteQuery(
            @"SELECT ct.MaSach, s.TenSach, ct.SoLuong, ct.DonGiaNhap, ct.ThanhTien
              FROM ChiTietPhieuNhap ct JOIN Sach s ON s.MaSach = ct.MaSach
              WHERE ct.MaPN = @MaPN",
            CommandType.Text, new SqlParameter("@MaPN", maPN));

    public string GetLastCode() =>
        DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaPN FROM PhieuNhap ORDER BY MaPN DESC")?.ToString();

    /// <summary>Lập chứng từ trong một giao dịch (Commit/Rollback). Lỗi SQL được dịch sang DataAccessException.</summary>
    public bool CreatePhieuNhap(PhieuNhap pn)
    {
        try
        {
            return CreatePhieuNhapCore(pn);
        }
        catch (SqlException ex)
        {
            throw SqlErrorTranslator.Dich(ex, "lập phiếu nhập");
        }
    }

    private bool CreatePhieuNhapCore(PhieuNhap pn)
    {
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            const string sqlPn = @"INSERT INTO PhieuNhap(MaPN, NgayNhap, MaNV, MaNCC, TongTien, GhiChu)
                                   VALUES(@MaPN, @NgayNhap, @MaNV, @MaNCC, @TongTien, @GhiChu)";
            using (var cmd = new SqlCommand(sqlPn, conn, tran))
            {
                cmd.Parameters.AddWithValue("@MaPN", pn.MaChungTu);
                cmd.Parameters.AddWithValue("@NgayNhap", pn.NgayLap);
                cmd.Parameters.AddWithValue("@MaNV", pn.MaNV);
                cmd.Parameters.AddWithValue("@MaNCC", pn.MaNCC);
                cmd.Parameters.AddWithValue("@TongTien", pn.TongTien);
                cmd.Parameters.AddWithValue("@GhiChu", (object)pn.GhiChu ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            const string sqlCt = @"INSERT INTO ChiTietPhieuNhap(MaPN, MaSach, SoLuong, DonGiaNhap)
                                   VALUES(@MaPN, @MaSach, @SoLuong, @DonGia)";
            foreach (var ct in pn.ChiTiet)
            {
                using var cmdCt = new SqlCommand(sqlCt, conn, tran);
                cmdCt.Parameters.AddWithValue("@MaPN", pn.MaChungTu);
                cmdCt.Parameters.AddWithValue("@MaSach", ct.MaSach);
                cmdCt.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                cmdCt.Parameters.AddWithValue("@DonGia", ct.DonGia);
                cmdCt.ExecuteNonQuery();
            }

            tran.Commit();
            return true;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
}
