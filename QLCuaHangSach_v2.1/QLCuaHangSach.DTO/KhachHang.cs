

namespace QLCuaHangSach.DTO;

/// <summary>Khách hàng thành viên - kế thừa từ ConNguoi.</summary>
public class KhachHang : ConNguoi, IPrintable
{
    public string MaKH { get; set; }
    public int DiemTichLuy { get; set; }
    public DateTime NgayDangKy { get; set; } = DateTime.Today;

    public override string Ma => MaKH;

    /// <summary>Hạng thành viên xác định theo điểm tích lũy.</summary>
    public string HangThanhVien => DiemTichLuy switch
    {
        >= 300 => "Kim cương",
        >= 150 => "Vàng",
        >= 50 => "Bạc",
        _ => "Đồng"
    };

    /// <summary>Tỷ lệ giảm giá được hưởng theo hạng thành viên.</summary>
    public decimal TyLeGiamGia() => DiemTichLuy switch
    {
        >= 300 => 0.10m,
        >= 150 => 0.07m,
        >= 50 => 0.05m,
        _ => 0.02m
    };

    public override string HienThi() => $"{HoTen} - {SoDienThoai}";

    /// <summary>Nội dung thẻ thành viên (thực thi IPrintable).</summary>
    public string ToPrintText() =>
        $"THẺ THÀNH VIÊN - NHÀ SÁCH ĐÔNG Á{Environment.NewLine}" +
        $"Mã: {MaKH}   Họ tên: {HoTen}{Environment.NewLine}" +
        $"SĐT: {SoDienThoai}{Environment.NewLine}" +
        $"Hạng: {HangThanhVien} ({TyLeGiamGia():P0})   Điểm tích lũy: {DiemTichLuy}";
}
