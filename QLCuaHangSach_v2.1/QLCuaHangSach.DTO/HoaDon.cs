

namespace QLCuaHangSach.DTO;

/// <summary>Hóa đơn bán hàng.</summary>
public class HoaDon : ChungTuBase<ChiTietHoaDon>
{
    public string MaKH { get; set; }
    public string TenKH { get; set; }
    public decimal GiamGia { get; set; }
    public decimal ThanhToan => TongTien - GiamGia;
    public decimal TienKhachDua { get; set; }
    public decimal TienThoi => TienKhachDua - ThanhToan;

    public override string LoaiChungTu => "Hóa đơn";

    protected override IEnumerable<string> DongTongKet()
    {
        string khach = string.IsNullOrWhiteSpace(TenKH) ? "Khách lẻ" : TenKH;
        yield return "Khách hàng: " + khach;
        yield return string.Format("Tổng tiền hàng:   {0,14:N0} đ", TongTien);
        yield return string.Format("Giảm giá:         {0,14:N0} đ", GiamGia);
        yield return string.Format("KHÁCH CẦN TRẢ:    {0,14:N0} đ", ThanhToan);
    }
}
