

namespace QLCuaHangSach.DTO;

/// <summary>Tác giả - kế thừa DanhMucBase (ThongTinThem = Quốc tịch).</summary>
public class TacGia : DanhMucBase
{
    public string QuocTich { get => ThongTinThem; set => ThongTinThem = value; }
}
