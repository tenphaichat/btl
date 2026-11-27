using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL;

/// <summary>Xử lý lưu trữ Hóa đơn, Phiếu nhập (có Transaction) và các truy vấn Thống kê.</summary>
public class HoaDonDAL
{
    public DataTable GetAll(DateTime? tuNgay = null, DateTime? denNgay = null)
    {
        const string sql = @"SELECT hd.MaHD, hd.NgayLap, nv.HoTen AS NhanVien,
                                    ISNULL(kh.HoTen, N'Khách lẻ') AS KhachHang,
                                    hd.TongTien, hd.GiamGia, hd.ThanhToan, hd.GhiChu
                             FROM HoaDon hd
                             JOIN NhanVien nv ON nv.MaNV = hd.MaNV
                             LEFT JOIN KhachHang kh ON kh.MaKH = hd.MaKH
                             WHERE (@Tu IS NULL OR CAST(hd.NgayLap AS DATE) >= @Tu)
                               AND (@Den IS NULL OR CAST(hd.NgayLap AS DATE) <= @Den)
                             ORDER BY hd.NgayLap DESC";
        return DatabaseConnection.ExecuteQuery(sql, CommandType.Text,
            new SqlParameter("@Tu", (object)tuNgay?.Date ?? DBNull.Value),
            new SqlParameter("@Den", (object)denNgay?.Date ?? DBNull.Value));
    }

    /// <summary>Đầu hóa đơn của một mã (qua sp_HoaDon_GetById) - dùng cho báo cáo in hóa đơn.</summary>
    public DataTable GetById(string maHD) =>
        DatabaseConnection.ExecuteQuery("sp_HoaDon_GetById", CommandType.StoredProcedure,
            DatabaseConnection.Param("@MaHD", maHD));

    public DataTable GetChiTiet(string maHD) =>
        DatabaseConnection.ExecuteQuery(
            @"SELECT ct.MaSach, s.TenSach, ct.SoLuong, ct.DonGia, ct.ThanhTien
              FROM ChiTietHoaDon ct JOIN Sach s ON s.MaSach = ct.MaSach
              WHERE ct.MaHD = @MaHD",
            CommandType.Text, new SqlParameter("@MaHD", maHD));

    public string GetLastCode() =>
        DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaHD FROM HoaDon ORDER BY MaHD DESC")?.ToString();

    /// <summary>Lưu hóa đơn và chi tiết trong một SqlTransaction (Trigger tự động trừ tồn kho).</summary>
    /// <summary>Lập chứng từ trong một giao dịch (Commit/Rollback). Lỗi SQL được dịch sang DataAccessException.</summary>
    public bool CreateHoaDon(HoaDon hd)
    {
        try
        {
            return CreateHoaDonCore(hd);
        }
        catch (SqlException ex)
        {
            throw SqlErrorTranslator.Dich(ex, "lập hóa đơn");
        }
    }

    private bool CreateHoaDonCore(HoaDon hd)
    {
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            const string sqlHd = @"INSERT INTO HoaDon(MaHD, NgayLap, MaNV, MaKH, TongTien, GiamGia, ThanhToan, GhiChu)
                                   VALUES(@MaHD, @NgayLap, @MaNV, @MaKH, @TongTien, @GiamGia, @ThanhToan, @GhiChu)";
            using (var cmd = new SqlCommand(sqlHd, conn, tran))
            {
                cmd.Parameters.AddWithValue("@MaHD", hd.MaChungTu);
                cmd.Parameters.AddWithValue("@NgayLap", hd.NgayLap);
                cmd.Parameters.AddWithValue("@MaNV", hd.MaNV);
                cmd.Parameters.AddWithValue("@MaKH", string.IsNullOrWhiteSpace(hd.MaKH) ? DBNull.Value : hd.MaKH);
                cmd.Parameters.AddWithValue("@TongTien", hd.TongTien);
                cmd.Parameters.AddWithValue("@GiamGia", hd.GiamGia);
                cmd.Parameters.AddWithValue("@ThanhToan", hd.ThanhToan);
                cmd.Parameters.AddWithValue("@GhiChu", (object)hd.GhiChu ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            const string sqlCt = @"INSERT INTO ChiTietHoaDon(MaHD, MaSach, SoLuong, DonGia)
                                   VALUES(@MaHD, @MaSach, @SoLuong, @DonGia)";
            foreach (var ct in hd.ChiTiet)
            {
                using var cmdCt = new SqlCommand(sqlCt, conn, tran);
                cmdCt.Parameters.AddWithValue("@MaHD", hd.MaChungTu);
                cmdCt.Parameters.AddWithValue("@MaSach", ct.MaSach);
                cmdCt.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                cmdCt.Parameters.AddWithValue("@DonGia", ct.DonGia);
                cmdCt.ExecuteNonQuery();
            }

            if (!string.IsNullOrWhiteSpace(hd.MaKH))
            {
                int diemCong = (int)(hd.ThanhToan / 10000m);
                using var cmdKh = new SqlCommand(
                    "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @Diem WHERE MaKH = @MaKH", conn, tran);
                cmdKh.Parameters.AddWithValue("@Diem", diemCong);
                cmdKh.Parameters.AddWithValue("@MaKH", hd.MaKH);
                cmdKh.ExecuteNonQuery();
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
