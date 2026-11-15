using System.Data;
using System.Data.SqlClient;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.DAL
{
    public class SachDAL
    {
        public DataTable Search(string keyword = "", string maTL = null) =>
            DatabaseConnection.ExecuteQuery("sp_TimKiemSach", CommandType.StoredProcedure,
                new SqlParameter("@TuKhoa", keyword ?? ""),
                new SqlParameter("@MaTL", string.IsNullOrWhiteSpace(maTL) ? DBNull.Value : maTL));

        public bool Insert(Sach s) =>
            DatabaseConnection.ExecuteNonQuery(
                @"INSERT INTO Sach(MaSach, TenSach, MaTL, MaTG, MaNXB, NamXuatBan, GiaNhap, GiaBan, SoLuongTon, MoTa, TrangThai)
                  VALUES(@MaSach, @TenSach, @MaTL, @MaTG, @MaNXB, @NamXB, @GiaNhap, @GiaBan, @SoLuongTon, @MoTa, 1)",
                CommandType.Text,
                new SqlParameter("@MaSach", s.MaSach),
                new SqlParameter("@TenSach", s.TenSach),
                new SqlParameter("@MaTL", s.MaTL),
                new SqlParameter("@MaTG", s.MaTG),
                new SqlParameter("@MaNXB", s.MaNXB),
                new SqlParameter("@NamXB", (object)s.NamXuatBan ?? DBNull.Value),
                new SqlParameter("@GiaNhap", s.GiaNhap),
                new SqlParameter("@GiaBan", s.GiaBan),
                new SqlParameter("@SoLuongTon", s.SoLuongTon),
                new SqlParameter("@MoTa", (object)s.MoTa ?? DBNull.Value)) > 0;

        public bool Update(Sach s) =>
            DatabaseConnection.ExecuteNonQuery(
                @"UPDATE Sach SET TenSach=@TenSach, MaTL=@MaTL, MaTG=@MaTG, MaNXB=@MaNXB,
                         NamXuatBan=@NamXB, GiaNhap=@GiaNhap, GiaBan=@GiaBan, SoLuongTon=@SoLuongTon, MoTa=@MoTa
                  WHERE MaSach=@MaSach",
                CommandType.Text,
                new SqlParameter("@MaSach", s.MaSach),
                new SqlParameter("@TenSach", s.TenSach),
                new SqlParameter("@MaTL", s.MaTL),
                new SqlParameter("@MaTG", s.MaTG),
                new SqlParameter("@MaNXB", s.MaNXB),
                new SqlParameter("@NamXB", (object)s.NamXuatBan ?? DBNull.Value),
                new SqlParameter("@GiaNhap", s.GiaNhap),
                new SqlParameter("@GiaBan", s.GiaBan),
                new SqlParameter("@SoLuongTon", s.SoLuongTon),
                new SqlParameter("@MoTa", (object)s.MoTa ?? DBNull.Value)) > 0;

        public bool Delete(string maSach) =>
            DatabaseConnection.ExecuteNonQuery("UPDATE Sach SET TrangThai = 0 WHERE MaSach = @MaSach",
                CommandType.Text, new SqlParameter("@MaSach", maSach)) > 0;

        public string GetLastCode() =>
            DatabaseConnection.ExecuteScalar("SELECT TOP 1 MaSach FROM Sach ORDER BY MaSach DESC")?.ToString();
    }

    public class DanhMucDAL
    {
        public DataTable GetTheLoai() =>
            DatabaseConnection.ExecuteQuery("SELECT MaTL, TenTL, MoTa FROM TheLoai ORDER BY MaTL");

        public DataTable GetTacGia() =>
            DatabaseConnection.ExecuteQuery("SELECT MaTG, TenTG, QuocTich FROM TacGia ORDER BY MaTG");

        public DataTable GetNhaXuatBan() =>
            DatabaseConnection.ExecuteQuery("SELECT MaNXB, TenNXB, DiaChi FROM NhaXuatBan ORDER BY MaNXB");

        public DataTable GetNhaCungCap() =>
            DatabaseConnection.ExecuteQuery("SELECT MaNCC, TenNCC, DiaChi, SoDienThoai, Email FROM NhaCungCap ORDER BY MaNCC");

        public bool SaveTheLoai(string ma, string ten, string moTa, bool isNew) =>
            DatabaseConnection.ExecuteNonQuery(
                isNew ? "INSERT INTO TheLoai(MaTL, TenTL, MoTa) VALUES(@Ma, @Ten, @MoTa)"
                      : "UPDATE TheLoai SET TenTL=@Ten, MoTa=@MoTa WHERE MaTL=@Ma",
                CommandType.Text,
                new SqlParameter("@Ma", ma),
                new SqlParameter("@Ten", ten),
                new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value)) > 0;

        public bool DeleteTheLoai(string ma) =>
            DatabaseConnection.ExecuteNonQuery("DELETE FROM TheLoai WHERE MaTL=@Ma",
                CommandType.Text, new SqlParameter("@Ma", ma)) > 0;

        public bool SaveTacGia(string ma, string ten, string quocTich, bool isNew) =>
            DatabaseConnection.ExecuteNonQuery(
                isNew ? "INSERT INTO TacGia(MaTG, TenTG, QuocTich) VALUES(@Ma, @Ten, @QT)"
                      : "UPDATE TacGia SET TenTG=@Ten, QuocTich=@QT WHERE MaTG=@Ma",
                CommandType.Text,
                new SqlParameter("@Ma", ma),
                new SqlParameter("@Ten", ten),
                new SqlParameter("@QT", (object)quocTich ?? DBNull.Value)) > 0;

        public bool SaveNhaXuatBan(string ma, string ten, string diaChi, bool isNew) =>
            DatabaseConnection.ExecuteNonQuery(
                isNew ? "INSERT INTO NhaXuatBan(MaNXB, TenNXB, DiaChi) VALUES(@Ma, @Ten, @DC)"
                      : "UPDATE NhaXuatBan SET TenNXB=@Ten, DiaChi=@DC WHERE MaNXB=@Ma",
                CommandType.Text,
                new SqlParameter("@Ma", ma),
                new SqlParameter("@Ten", ten),
                new SqlParameter("@DC", (object)diaChi ?? DBNull.Value)) > 0;

        public bool SaveNhaCungCap(NhaCungCap ncc, bool isNew) =>
            DatabaseConnection.ExecuteNonQuery(
                isNew ? "INSERT INTO NhaCungCap(MaNCC, TenNCC, DiaChi, SoDienThoai, Email) VALUES(@Ma, @Ten, @DC, @SDT, @Email)"
                      : "UPDATE NhaCungCap SET TenNCC=@Ten, DiaChi=@DC, SoDienThoai=@SDT, Email=@Email WHERE MaNCC=@Ma",
                CommandType.Text,
                new SqlParameter("@Ma", ncc.MaNCC),
                new SqlParameter("@Ten", ncc.TenNCC),
                new SqlParameter("@DC", (object)ncc.DiaChi ?? DBNull.Value),
                new SqlParameter("@SDT", (object)ncc.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@Email", (object)ncc.Email ?? DBNull.Value)) > 0;
    }
}
