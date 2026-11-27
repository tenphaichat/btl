using QLCuaHangSach.DTO;

namespace QLCuaHangSach.BUS;

/// <summary>Giảm đồng giá cho mọi khách thành viên khi đơn từ mức tối thiểu (mặc định 5% cho đơn từ 500.000đ).</summary>
public class FlatRateDiscountPolicy : IDiscountPolicy
{
    private readonly decimal _tyLe;
    private readonly decimal _tongToiThieu;

    public FlatRateDiscountPolicy(decimal tyLe = 0.05m, decimal tongToiThieu = 500000m)
    {
        _tyLe = tyLe;
        _tongToiThieu = tongToiThieu;
    }

    public string Ten => "Giảm đồng giá cho thành viên";

    public decimal TyLeGiamGia(decimal tongTien, KhachHang khachHang) =>
        khachHang == null || tongTien < _tongToiThieu ? 0m : _tyLe;
}
