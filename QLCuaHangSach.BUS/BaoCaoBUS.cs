using System.Data;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>
/// Chuẩn bị dữ liệu cho các báo cáo Crystal Report.
/// Mỗi báo cáo là một DataSet gồm: bảng dữ liệu chính (tên cố định, khớp file .xsd/.rpt) và bảng "ThamSo"
/// (Ten, GiaTri) - các tham số truyền vào Parameter Field của báo cáo (Lab 14).
/// </summary>
public class BaoCaoBUS
{
    public const string BANG_HOADON = "HoaDonChiTiet";
    public const string BANG_DOANHTHU = "DoanhThuThang";
    public const string BANG_SACHBANCHAY = "SachBanChay";
    public const string BANG_THAMSO = "ThamSo";

    private readonly HoaDonDAL _hoaDonDal = new HoaDonDAL();
    private readonly ThongKeDAL _tkDal = new ThongKeDAL();

    /// <summary>Báo cáo "Hóa đơn bán hàng": mỗi dòng chi tiết kèm các cột đầu hóa đơn (không cần liên kết bảng trong Crystal).</summary>
    public DataSet DuLieuHoaDon(string maHD)
    {
        PhanQuyenBUS.YeuCau(ManHinh.BanHang);
        DataTable dau = _hoaDonDal.GetById(maHD);
        if (dau.Rows.Count == 0) throw new BusinessException($"Không tìm thấy hóa đơn {maHD}.");
        DataRow h = dau.Rows[0];
        DataTable ct = _hoaDonDal.GetChiTiet(maHD);

        var bang = new DataTable(BANG_HOADON);
        bang.Columns.Add("MaHD", typeof(string));
        bang.Columns.Add("NgayLap", typeof(DateTime));
        bang.Columns.Add("NhanVien", typeof(string));
        bang.Columns.Add("KhachHang", typeof(string));
        bang.Columns.Add("TongTien", typeof(decimal));
        bang.Columns.Add("GiamGia", typeof(decimal));
        bang.Columns.Add("ThanhToan", typeof(decimal));
        bang.Columns.Add("STT", typeof(int));
        bang.Columns.Add("MaSach", typeof(string));
        bang.Columns.Add("TenSach", typeof(string));
        bang.Columns.Add("SoLuong", typeof(int));
        bang.Columns.Add("DonGia", typeof(decimal));
        bang.Columns.Add("ThanhTien", typeof(decimal));

        int stt = 0;
        foreach (DataRow r in ct.Rows)
        {
            stt++;
            bang.Rows.Add(h["MaHD"], h["NgayLap"], h["NhanVien"], h["KhachHang"],
                          h["TongTien"], h["GiamGia"], h["ThanhToan"],
                          stt, r["MaSach"], r["TenSach"], r["SoLuong"], r["DonGia"], r["ThanhTien"]);
        }

        AuditBUS.Ghi("IN_HOA_DON", maHD);
        return GoiThamSo(bang, ("pTieuDe", "HÓA ĐƠN BÁN HÀNG"), ("pNguoiLap", NguoiLap()), ("pNgayLap", NgayLap()));
    }

    /// <summary>Báo cáo "Doanh thu theo tháng" của một năm.</summary>
    public DataSet DuLieuDoanhThuThang(int nam)
    {
        PhanQuyenBUS.YeuCau(ManHinh.ThongKe);
        DataTable src = _tkDal.DoanhThuTheoThang(nam);
        DataTable bang = src.Copy();
        bang.TableName = BANG_DOANHTHU;
        AuditBUS.Ghi("IN_BAO_CAO", "DoanhThuThang", "Năm " + nam);
        return GoiThamSo(bang, ("pTieuDe", "BÁO CÁO DOANH THU THEO THÁNG"), ("pNam", nam.ToString()),
                         ("pNguoiLap", NguoiLap()), ("pNgayLap", NgayLap()));
    }

    /// <summary>Báo cáo "Sách bán chạy" theo khoảng thời gian.</summary>
    public DataSet DuLieuSachBanChay(DateTime tuNgay, DateTime denNgay, int top = 10)
    {
        PhanQuyenBUS.YeuCau(ManHinh.ThongKe);
        if (!ValidationHelper.IsValidDateRange(tuNgay, denNgay))
            throw new BusinessException("Từ ngày phải nhỏ hơn hoặc bằng Đến ngày.");
        DataTable bang = _tkDal.SachBanChay(tuNgay, denNgay, top).Copy();
        bang.TableName = BANG_SACHBANCHAY;
        AuditBUS.Ghi("IN_BAO_CAO", "SachBanChay", $"{tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy}");
        return GoiThamSo(bang, ("pTieuDe", "BÁO CÁO SÁCH BÁN CHẠY"),
                         ("pTuNgay", tuNgay.ToString("dd/MM/yyyy")), ("pDenNgay", denNgay.ToString("dd/MM/yyyy")),
                         ("pNguoiLap", NguoiLap()), ("pNgayLap", NgayLap()));
    }

    private static string NguoiLap() => SessionManager.CurrentUser?.HoTen ?? "";
    private static string NgayLap() => DateTime.Now.ToString("dd/MM/yyyy HH:mm");

    /// <summary>Đóng gói bảng dữ liệu chính và các tham số (tên, giá trị) vào một DataSet.</summary>
    private static DataSet GoiThamSo(DataTable bangChinh, params (string Ten, string GiaTri)[] thamSo)
    {
        var ts = new DataTable(BANG_THAMSO);
        ts.Columns.Add("Ten", typeof(string));
        ts.Columns.Add("GiaTri", typeof(string));
        foreach (var (ten, giaTri) in thamSo) ts.Rows.Add(ten, giaTri);

        var ds = new DataSet("BaoCao");
        ds.Tables.Add(bangChinh);
        ds.Tables.Add(ts);
        return ds;
    }
}
