

namespace QLCuaHangSach.DTO;

/// <summary>Nhà xuất bản - kế thừa DanhMucBase (ThongTinThem = Địa chỉ).</summary>
public class NhaXuatBan : DanhMucBase
{
    public string DiaChi { get => ThongTinThem; set => ThongTinThem = value; }
}
