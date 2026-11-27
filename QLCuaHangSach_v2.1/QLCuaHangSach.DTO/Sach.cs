

namespace QLCuaHangSach.DTO;

/// <summary>Đầu sách được kinh doanh tại cửa hàng.</summary>
public class Sach : EntityBase
{
    public string MaSach { get; set; }
    public string TenSach { get; set; }
    public string MaTL { get; set; }
    public string TenTL { get; set; }
    public string MaTG { get; set; }
    public string TenTG { get; set; }
    public string MaNXB { get; set; }
    public string TenNXB { get; set; }
    public int? NamXuatBan { get; set; }
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string MoTa { get; set; }
    public bool TrangThai { get; set; } = true;

    public override string Ma => MaSach;
    public override string HienThi() => $"{MaSach} - {TenSach}";

    /// <summary>Lợi nhuận dự kiến trên mỗi cuốn sách.</summary>
    public decimal LoiNhuanMoiCuon => GiaBan - GiaNhap;

    public bool ConHang(int soLuong = 1) => SoLuongTon >= soLuong;

    protected override string KiemTraDuLieu() =>
        base.KiemTraDuLieu()
        ?? (string.IsNullOrWhiteSpace(TenSach) ? "Tên sách không được để trống." : null)
        ?? (GiaNhap < 0 ? "Giá nhập không được âm." : null)
        ?? (GiaBan < 0 ? "Giá bán không được âm." : null);
}
