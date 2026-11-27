using QLCuaHangSach.DTO;

namespace QLCuaHangSach.BUS;

/// <summary>
/// Hợp đồng chính sách giảm giá. BanHangBUS chỉ làm việc với interface này,
/// muốn đổi cách tính giảm giá chỉ cần đổi lớp cài đặt, không phải sửa nơi khác.
/// </summary>
public interface IDiscountPolicy
{
    string Ten { get; }

    /// <summary>Tỷ lệ giảm (0.05 = 5%); 0 nếu không được giảm. khachHang = null nghĩa là khách lẻ.</summary>
    decimal TyLeGiamGia(decimal tongTien, KhachHang khachHang);

    /// <summary>Số tiền giảm, làm tròn đến nghìn đồng.</summary>
    decimal TinhGiamGia(decimal tongTien, KhachHang khachHang) =>
        Math.Round(tongTien * TyLeGiamGia(tongTien, khachHang) / 1000m, 0, MidpointRounding.AwayFromZero) * 1000m;
}
