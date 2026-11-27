using System.Data;
using System.Text;



namespace QLCuaHangSach.DTO;

/// <summary>
/// Lớp chứng từ tổng quát (generic) - lớp cha của HoaDon và PhieuNhap.
/// Một chứng từ gồm phần đầu (mã, ngày, nhân viên) và danh sách dòng chi tiết.
/// </summary>
public abstract class ChungTuBase<TChiTiet> : EntityBase, IPrintable, IExportable where TChiTiet : ChiTietBase
{
    public string MaChungTu { get; set; }
    public DateTime NgayLap { get; set; } = DateTime.Now;
    public string MaNV { get; set; }
    public string TenNV { get; set; }
    public decimal TongTien { get; set; }
    public string GhiChu { get; set; }
    public List<TChiTiet> ChiTiet { get; } = new List<TChiTiet>();

    public override string Ma => MaChungTu;

    /// <summary>Tên loại chứng từ (Hóa đơn bán hàng, Phiếu nhập kho...).</summary>
    public abstract string LoaiChungTu { get; }

    /// <summary>Tính lại tổng tiền dựa trên danh sách chi tiết.</summary>
    public virtual decimal TinhTongTien()
    {
        TongTien = ChiTiet.Sum(c => c.ThanhTien);
        return TongTien;
    }

    public int TongSoLuong => ChiTiet.Sum(c => c.SoLuong);

    /// <summary>Thêm sách vào chứng từ; nếu đã có thì cộng dồn số lượng.</summary>
    public void ThemChiTiet(TChiTiet ct)
    {
        TChiTiet daCo = ChiTiet.FirstOrDefault(x => x.MaSach == ct.MaSach);
        if (daCo != null) daCo.SoLuong += ct.SoLuong;
        else ChiTiet.Add(ct);
        TinhTongTien();
    }

    public void XoaChiTiet(string maSach)
    {
        ChiTiet.RemoveAll(x => x.MaSach == maSach);
        TinhTongTien();
    }

    public override string HienThi() => $"{LoaiChungTu} {MaChungTu} ({NgayLap:dd/MM/yyyy})";

    /// <summary>Các dòng tổng kết ở cuối chứng từ; lớp con (HoaDon) ghi đè để thêm giảm giá, thanh toán.</summary>
    protected virtual IEnumerable<string> DongTongKet()
    {
        yield return string.Format("TỔNG TIỀN: {0:N0} đ", TongTien);
    }

    /// <summary>Thực thi IPrintable: nội dung chứng từ dạng văn bản để xem trước/in.</summary>
    public string ToPrintText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("NHÀ SÁCH ĐÔNG Á");
        sb.AppendLine(LoaiChungTu.ToUpper() + ": " + MaChungTu);
        sb.AppendLine(string.Format("Ngày lập: {0:dd/MM/yyyy HH:mm}   Nhân viên: {1}", NgayLap, string.IsNullOrWhiteSpace(TenNV) ? MaNV : TenNV));
        sb.AppendLine(new string('-', 56));
        sb.AppendLine(string.Format("{0,-26}{1,4}{2,13}{3,13}", "Tên sách", "SL", "Đơn giá", "Thành tiền"));
        foreach (TChiTiet c in ChiTiet)
        {
            string ten = string.IsNullOrWhiteSpace(c.TenSach) ? c.MaSach : c.TenSach;
            if (ten.Length > 24) ten = ten.Substring(0, 23) + "…";
            sb.AppendLine(string.Format("{0,-26}{1,4}{2,13:N0}{3,13:N0}", ten, c.SoLuong, c.DonGia, c.ThanhTien));
        }
        sb.AppendLine(new string('-', 56));
        foreach (string dong in DongTongKet()) sb.AppendLine(dong);
        return sb.ToString();
    }

    /// <summary>Thực thi IExportable: các dòng chi tiết dưới dạng DataTable (nguồn cho Excel/Crystal Report).</summary>
    public DataTable ToDataTable()
    {
        var dt = new DataTable("ChiTietChungTu");
        dt.Columns.Add("MaSach", typeof(string));
        dt.Columns.Add("TenSach", typeof(string));
        dt.Columns.Add("SoLuong", typeof(int));
        dt.Columns.Add("DonGia", typeof(decimal));
        dt.Columns.Add("ThanhTien", typeof(decimal));
        foreach (TChiTiet c in ChiTiet)
            dt.Rows.Add(c.MaSach, c.TenSach, c.SoLuong, c.DonGia, c.ThanhTien);
        return dt;
    }
}
