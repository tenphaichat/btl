using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL
{
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

        public DataTable GetChiTiet(string maHD) =>
            DatabaseConnection.ExecuteQuery(
                @"SELECT ct.MaSach, s.TenSach, ct.SoLuong, ct.DonGia, ct.ThanhTien
                  FROM ChiTietHoaDon ct JOIN Sach s ON s.MaSach = ct.MaSach
                  WHERE ct.MaHD = @MaHD",
                CommandType.Text, new SqlParameter("@MaHD", maHD));

        public string GetLastCode() =>
            DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaHD FROM HoaDon ORDER BY MaHD DESC")?.ToString();

        /// <summary>Lưu hóa đơn và chi tiết trong một SqlTransaction (Trigger tự động trừ tồn kho).</summary>
        public bool CreateHoaDon(HoaDon hd)
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

        public bool CreatePhieuNhap(PhieuNhap pn)
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

    public class ThongKeDAL
    {
        public DataTable GetTongQuan() =>
            DatabaseConnection.ExecuteQuery("sp_ThongKeTongQuan", CommandType.StoredProcedure);

        public DataTable TimHoaDonGiaTriCaoNhat(DateTime? tuNgay = null, DateTime? denNgay = null) =>
            DatabaseConnection.ExecuteQuery("sp_TimHoaDonGiaTriCaoNhat", CommandType.StoredProcedure,
                new SqlParameter("@TuNgay", (object)tuNgay?.Date ?? DBNull.Value),
                new SqlParameter("@DenNgay", (object)denNgay?.Date ?? DBNull.Value));

        public DataTable SachBanChay(DateTime tuNgay, DateTime denNgay, int top = 10) =>
            DatabaseConnection.ExecuteQuery("sp_SachBanChayTheoKhoangThoiGian", CommandType.StoredProcedure,
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date),
                new SqlParameter("@Top", top));

        public DataTable DoanhThuTheoThang(int nam) =>
            DatabaseConnection.ExecuteQuery("sp_DoanhThuTheoThang", CommandType.StoredProcedure,
                new SqlParameter("@Nam", nam));

        public DataTable SachSapHetHang(int nguong = 10) =>
            DatabaseConnection.ExecuteQuery("sp_SachSapHetHang", CommandType.StoredProcedure,
                new SqlParameter("@NguongTon", nguong));

        public DataTable TopKhachHang(int top = 10) =>
            DatabaseConnection.ExecuteQuery("sp_TopKhachHang", CommandType.StoredProcedure,
                new SqlParameter("@Top", top));
    }
}
