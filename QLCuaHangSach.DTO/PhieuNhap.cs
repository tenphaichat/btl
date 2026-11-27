

namespace QLCuaHangSach.DTO;

/// <summary>Phiếu nhập hàng từ nhà cung cấp.</summary>
public class PhieuNhap : ChungTuBase<ChiTietPhieuNhap>
{
    public string MaNCC { get; set; }
    public string TenNCC { get; set; }

    public override string LoaiChungTu => "Phiếu nhập";
}
