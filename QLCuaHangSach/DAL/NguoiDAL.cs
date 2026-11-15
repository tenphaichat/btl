using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL
{
    /// <summary>Tầng truy cập dữ liệu cho Nhân viên, Khách hàng và Danh mục.</summary>
    public class NhanVienDAL
    {
        public DataTable GetAll() =>
            DatabaseConnection.ExecuteQuery(
                @"SELECT MaNV, HoTen, CASE WHEN GioiTinh = 1 THEN N'Nam' ELSE N'Nữ' END AS GioiTinh,
                         NgaySinh, SoDienThoai, Email, DiaChi, TenDangNhap, VaiTro, TrangThai
                  FROM NhanVien ORDER BY MaNV");

        public NhanVien GetByLogin(string username, string passwordHash)
        {
            const string sql = @"SELECT TOP 1 * FROM NhanVien
                                 WHERE TenDangNhap = @User AND MatKhauHash = @Hash AND TrangThai = 1";
            DataTable dt = DatabaseConnection.ExecuteQuery(sql, CommandType.Text,
                new SqlParameter("@User", username),
                new SqlParameter("@Hash", passwordHash));

            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new NhanVien
            {
                MaNV = r["MaNV"].ToString(),
                HoTen = r["HoTen"].ToString(),
                GioiTinh = Convert.ToBoolean(r["GioiTinh"]),
                NgaySinh = r["NgaySinh"] == DBNull.Value ? null : Convert.ToDateTime(r["NgaySinh"]),
                SoDienThoai = r["SoDienThoai"]?.ToString(),
                Email = r["Email"]?.ToString(),
                DiaChi = r["DiaChi"]?.ToString(),
                TenDangNhap = r["TenDangNhap"].ToString(),
                MatKhauHash = r["MatKhauHash"].ToString(),
                VaiTro = r["VaiTro"].ToString(),
                TrangThai = Convert.ToBoolean(r["TrangThai"])
            };
        }

        public bool Insert(NhanVien nv) =>
            DatabaseConnection.ExecuteNonQuery(
                @"INSERT INTO NhanVien(MaNV, HoTen, GioiTinh, NgaySinh, SoDienThoai, Email, DiaChi, TenDangNhap, MatKhauHash, VaiTro, TrangThai)
                  VALUES(@MaNV, @HoTen, @GioiTinh, @NgaySinh, @SDT, @Email, @DiaChi, @User, @Hash, @VaiTro, @TrangThai)",
                CommandType.Text,
                new SqlParameter("@MaNV", nv.MaNV),
                new SqlParameter("@HoTen", nv.HoTen),
                new SqlParameter("@GioiTinh", nv.GioiTinh),
                new SqlParameter("@NgaySinh", (object)nv.NgaySinh ?? DBNull.Value),
                new SqlParameter("@SDT", (object)nv.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@Email", (object)nv.Email ?? DBNull.Value),
                new SqlParameter("@DiaChi", (object)nv.DiaChi ?? DBNull.Value),
                new SqlParameter("@User", nv.TenDangNhap),
                new SqlParameter("@Hash", nv.MatKhauHash),
                new SqlParameter("@VaiTro", nv.VaiTro),
                new SqlParameter("@TrangThai", nv.TrangThai)) > 0;

        public bool Update(NhanVien nv) =>
            DatabaseConnection.ExecuteNonQuery(
                @"UPDATE NhanVien SET HoTen=@HoTen, GioiTinh=@GioiTinh, NgaySinh=@NgaySinh,
                         SoDienThoai=@SDT, Email=@Email, DiaChi=@DiaChi, VaiTro=@VaiTro, TrangThai=@TrangThai
                  WHERE MaNV=@MaNV",
                CommandType.Text,
                new SqlParameter("@MaNV", nv.MaNV),
                new SqlParameter("@HoTen", nv.HoTen),
                new SqlParameter("@GioiTinh", nv.GioiTinh),
                new SqlParameter("@NgaySinh", (object)nv.NgaySinh ?? DBNull.Value),
                new SqlParameter("@SDT", (object)nv.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@Email", (object)nv.Email ?? DBNull.Value),
                new SqlParameter("@DiaChi", (object)nv.DiaChi ?? DBNull.Value),
                new SqlParameter("@VaiTro", nv.VaiTro),
                new SqlParameter("@TrangThai", nv.TrangThai)) > 0;

        public bool Delete(string maNV) =>
            DatabaseConnection.ExecuteNonQuery("UPDATE NhanVien SET TrangThai = 0 WHERE MaNV = @MaNV",
                CommandType.Text, new SqlParameter("@MaNV", maNV)) > 0;
    }

    public class KhachHangDAL
    {
        public DataTable GetAll(string keyword = "")
        {
            const string sql = @"SELECT MaKH, HoTen, CASE WHEN GioiTinh = 1 THEN N'Nam' ELSE N'Nữ' END AS GioiTinh,
                                        NgaySinh, SoDienThoai, Email, DiaChi, DiemTichLuy, NgayDangKy
                                 FROM KhachHang
                                 WHERE @Kw = N'' OR HoTen LIKE N'%' + @Kw + N'%' OR SoDienThoai LIKE '%' + @Kw + '%' OR MaKH LIKE '%' + @Kw + '%'
                                 ORDER BY MaKH";
            return DatabaseConnection.ExecuteQuery(sql, CommandType.Text, new SqlParameter("@Kw", keyword ?? ""));
        }

        public bool Insert(KhachHang kh) =>
            DatabaseConnection.ExecuteNonQuery(
                @"INSERT INTO KhachHang(MaKH, HoTen, GioiTinh, NgaySinh, SoDienThoai, Email, DiaChi, DiemTichLuy)
                  VALUES(@MaKH, @HoTen, @GioiTinh, @NgaySinh, @SDT, @Email, @DiaChi, @Diem)",
                CommandType.Text,
                new SqlParameter("@MaKH", kh.MaKH),
                new SqlParameter("@HoTen", kh.HoTen),
                new SqlParameter("@GioiTinh", kh.GioiTinh),
                new SqlParameter("@NgaySinh", (object)kh.NgaySinh ?? DBNull.Value),
                new SqlParameter("@SDT", kh.SoDienThoai),
                new SqlParameter("@Email", (object)kh.Email ?? DBNull.Value),
                new SqlParameter("@DiaChi", (object)kh.DiaChi ?? DBNull.Value),
                new SqlParameter("@Diem", kh.DiemTichLuy)) > 0;

        public bool Update(KhachHang kh) =>
            DatabaseConnection.ExecuteNonQuery(
                @"UPDATE KhachHang SET HoTen=@HoTen, GioiTinh=@GioiTinh, NgaySinh=@NgaySinh,
                         SoDienThoai=@SDT, Email=@Email, DiaChi=@DiaChi, DiemTichLuy=@Diem
                  WHERE MaKH=@MaKH",
                CommandType.Text,
                new SqlParameter("@MaKH", kh.MaKH),
                new SqlParameter("@HoTen", kh.HoTen),
                new SqlParameter("@GioiTinh", kh.GioiTinh),
                new SqlParameter("@NgaySinh", (object)kh.NgaySinh ?? DBNull.Value),
                new SqlParameter("@SDT", kh.SoDienThoai),
                new SqlParameter("@Email", (object)kh.Email ?? DBNull.Value),
                new SqlParameter("@DiaChi", (object)kh.DiaChi ?? DBNull.Value),
                new SqlParameter("@Diem", kh.DiemTichLuy)) > 0;

        public bool Delete(string maKH) =>
            DatabaseConnection.ExecuteNonQuery("DELETE FROM KhachHang WHERE MaKH=@MaKH",
                CommandType.Text, new SqlParameter("@MaKH", maKH)) > 0;

        public string GetLastCode() =>
            DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaKH FROM KhachHang ORDER BY MaKH DESC")?.ToString();
    }
}
