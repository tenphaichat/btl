using QLCuaHangSach.DTO;

namespace QLCuaHangSach.BUS;

/// <summary>Giảm giá theo hạng thành viên (Đồng 2%, Bạc 5%, Vàng 7%, Kim cương 10%) khi đơn từ mức tối thiểu.</summary>
public class RankDiscountPolicy : IDiscountPolicy
{
    private readonly decimal _tongToiThieu;

    public RankDiscountPolicy(decimal tongToiThieu = 500000m) => _tongToiThieu = tongToiThieu;

    public string Ten => "Giảm theo hạng thành viên";

    public decimal TyLeGiamGia(decimal tongTien, KhachHang khachHang) =>
        khachHang == null || tongTien < _tongToiThieu ? 0m : khachHang.TyLeGiamGia();
}
