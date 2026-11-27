

namespace QLCuaHangSach.DTO;

/// <summary>
/// Lớp trừu tượng cho các danh mục đơn giản gồm Mã - Tên - Thông tin thêm
/// (lớp cha của TheLoai, TacGia, NhaXuatBan).
/// </summary>
public abstract class DanhMucBase : EntityBase
{
    public string MaDM { get; set; }
    public string Ten { get; set; }
    public string ThongTinThem { get; set; }

    public override string Ma => MaDM;
    public override string HienThi() => Ten;
    protected override string KiemTraDuLieu() =>
        base.KiemTraDuLieu() ?? (string.IsNullOrWhiteSpace(Ten) ? "Tên không được để trống." : null);
}
