using System.Data;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS
{
    public class AuthBUS
    {
        private readonly NhanVienDAL _dal = new NhanVienDAL();

        public NhanVien Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");

            string hash = SecurityHelper.HashPasswordSHA256(password.Trim());
            NhanVien nv = _dal.GetByLogin(username.Trim(), hash);
            if (nv == null)
                throw new InvalidOperationException("Tên đăng nhập hoặc mật khẩu không chính xác, hoặc tài khoản đã khóa.");

            SessionManager.Login(nv);
            return nv;
        }

        public DataTable GetAllNhanVien() => _dal.GetAll();

        public bool SaveNhanVien(NhanVien nv, string rawPassword, bool isNew)
        {
            if (!nv.HopLe())
                throw new ArgumentException("Mã nhân viên, họ tên và tên đăng nhập không được để trống.");
            if (!string.IsNullOrWhiteSpace(nv.SoDienThoai) && !ValidationHelper.IsValidPhone(nv.SoDienThoai))
                throw new ArgumentException("Số điện thoại nhân viên không hợp lệ (10-11 chữ số, bắt đầu bằng 0).");

            if (isNew)
            {
                nv.MatKhauHash = SecurityHelper.HashPasswordSHA256(string.IsNullOrWhiteSpace(rawPassword) ? "123456" : rawPassword);
                return _dal.Insert(nv);
            }
            return _dal.Update(nv);
        }

        public bool LockNhanVien(string maNV) => _dal.Delete(maNV);
    }

    public class SachBUS
    {
        private readonly SachDAL _dal = new SachDAL();

        public DataTable TimKiem(string keyword = "", string maTL = null) =>
            _dal.Search(keyword, maTL);

        public string SinhMaSachMoi() =>
            FormatHelper.GenerateNextCode("S", _dal.GetLastCode(), 3);

        public bool LuuSach(Sach s, bool isNew)
        {
            if (!s.HopLe())
                throw new ArgumentException("Thông tin sách chưa hợp lệ. Vui lòng kiểm tra mã, tên sách và giá.");
            if (s.GiaBan < s.GiaNhap)
                throw new ArgumentException("Giá bán phải lớn hơn hoặc bằng giá nhập.");
            return isNew ? _dal.Insert(s) : _dal.Update(s);
        }

        public bool NgungKinhDoanh(string maSach) => _dal.Delete(maSach);
    }

    public class DanhMucBUS
    {
        private readonly DanhMucDAL _dal = new DanhMucDAL();

        public DataTable GetTheLoai() => _dal.GetTheLoai();
        public DataTable GetTacGia() => _dal.GetTacGia();
        public DataTable GetNhaXuatBan() => _dal.GetNhaXuatBan();
        public DataTable GetNhaCungCap() => _dal.GetNhaCungCap();

        public bool LuuTheLoai(string ma, string ten, string moTa, bool isNew) => _dal.SaveTheLoai(ma, ten, moTa, isNew);
        public bool XoaTheLoai(string ma) => _dal.DeleteTheLoai(ma);
        public bool LuuTacGia(string ma, string ten, string quocTich, bool isNew) => _dal.SaveTacGia(ma, ten, quocTich, isNew);
        public bool LuuNhaXuatBan(string ma, string ten, string diaChi, bool isNew) => _dal.SaveNhaXuatBan(ma, ten, diaChi, isNew);
        public bool LuuNhaCungCap(NhaCungCap ncc, bool isNew) => _dal.SaveNhaCungCap(ncc, isNew);
    }

    public class KhachHangBUS
    {
        private readonly KhachHangDAL _dal = new KhachHangDAL();

        public DataTable GetAll(string keyword = "") => _dal.GetAll(keyword);

        public string SinhMaKHMoi() =>
            FormatHelper.GenerateNextCode("KH", _dal.GetLastCode(), 3);

        public bool LuuKhachHang(KhachHang kh, bool isNew)
        {
            if (!kh.HopLe())
                throw new ArgumentException("Mã khách hàng và họ tên không được để trống.");
            if (!ValidationHelper.IsValidPhone(kh.SoDienThoai))
                throw new ArgumentException("Số điện thoại không hợp lệ (10-11 số, bắt đầu bằng 0).");
            if (!ValidationHelper.IsValidEmail(kh.Email))
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");

            return isNew ? _dal.Insert(kh) : _dal.Update(kh);
        }

        public bool XoaKhachHang(string maKH) => _dal.Delete(maKH);
    }

    public class BanHangBUS
    {
        private readonly HoaDonDAL _dal = new HoaDonDAL();

        public DataTable GetDanhSachHoaDon(DateTime? tu = null, DateTime? den = null) => _dal.GetAll(tu, den);
        public DataTable GetChiTietHoaDon(string maHD) => _dal.GetChiTiet(maHD);

        public string SinhMaHoaDonMoi() =>
            FormatHelper.GenerateNextCode("HD", _dal.GetLastCode(), 4);

        /// <summary>Tính mức giảm giá tự động: thành viên mua từ 500.000đ được giảm 5% (làm tròn nghìn đồng).</summary>
        public decimal TinhGiamGia(decimal tongTien, string maKH)
        {
            if (!string.IsNullOrWhiteSpace(maKH) && tongTien >= 500000m)
                return Math.Round(tongTien * 0.05m / 1000m, 0) * 1000m;
            return 0m;
        }

        public bool ThanhToanHoaDon(HoaDon hd)
        {
            if (hd.ChiTiet.Count == 0)
                throw new InvalidOperationException("Giỏ hàng đang trống. Vui lòng chọn ít nhất một đầu sách.");
            hd.TinhTongTien();
            return _dal.CreateHoaDon(hd);
        }
    }

    public class NhapHangBUS
    {
        private readonly PhieuNhapDAL _dal = new PhieuNhapDAL();

        public DataTable GetDanhSachPhieuNhap() => _dal.GetAll();
        public DataTable GetChiTietPhieuNhap(string maPN) => _dal.GetChiTiet(maPN);

        public string SinhMaPhieuNhapMoi() =>
            FormatHelper.GenerateNextCode("PN", _dal.GetLastCode(), 4);

        public bool LuuPhieuNhap(PhieuNhap pn)
        {
            if (pn.ChiTiet.Count == 0)
                throw new InvalidOperationException("Phiếu nhập phải có ít nhất một mặt hàng.");
            pn.TinhTongTien();
            return _dal.CreatePhieuNhap(pn);
        }
    }

    public class ThongKeBUS
    {
        private readonly ThongKeDAL _dal = new ThongKeDAL();

        public DataRow GetTongQuan()
        {
            DataTable dt = _dal.GetTongQuan();
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public DataTable TimHoaDonGiaTriCaoNhat(DateTime? tu = null, DateTime? den = null) =>
            _dal.TimHoaDonGiaTriCaoNhat(tu, den);

        public DataTable SachBanChay(DateTime tu, DateTime den, int top = 10) =>
            _dal.SachBanChay(tu, den, top);

        public DataTable DoanhThuTheoThang(int nam) =>
            _dal.DoanhThuTheoThang(nam);

        public DataTable SachSapHetHang(int nguong = 10) =>
            _dal.SachSapHetHang(nguong);

        public DataTable TopKhachHang(int top = 10) =>
            _dal.TopKhachHang(top);
    }
}
