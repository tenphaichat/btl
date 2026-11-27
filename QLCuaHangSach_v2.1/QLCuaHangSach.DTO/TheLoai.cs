

namespace QLCuaHangSach.DTO;

/// <summary>Thể loại sách - kế thừa DanhMucBase (ThongTinThem = Mô tả).</summary>
public class TheLoai : DanhMucBase
{
    public string MoTa { get => ThongTinThem; set => ThongTinThem = value; }
}
