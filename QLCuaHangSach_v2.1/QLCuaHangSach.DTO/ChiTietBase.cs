

namespace QLCuaHangSach.DTO;

/// <summary>Lớp cơ sở cho một dòng chi tiết chứng từ (hóa đơn / phiếu nhập).</summary>
public abstract class ChiTietBase
{
    public string MaSach { get; set; }
    public string TenSach { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }

    public decimal ThanhTien => SoLuong * DonGia;
}
