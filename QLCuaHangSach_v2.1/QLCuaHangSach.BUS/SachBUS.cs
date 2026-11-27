using System.Data;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>Nghiệp vụ quản lý đầu sách.</summary>
public class SachBUS
{
    private readonly SachDAL _dal = new SachDAL();

    public DataTable TimKiem(string keyword = "", string maTL = null) =>
        _dal.Search(keyword, maTL);

    /// <summary>Các cột được phép dùng để sắp xếp (danh sách hợp lệ, không nhận tên cột tùy ý).</summary>
    private static readonly string[] CotSapXepHopLe = { "MaSach", "TenSach", "GiaBan", "SoLuongTon" };

    /// <summary>Tìm kiếm đa tiêu chí (từ khóa, thể loại, khoảng giá) có sắp xếp và phân trang.</summary>
    public DataTable TimKiemPhanTrang(string keyword, string maTL, decimal? giaTu, decimal? giaDen,
                                      string sapXep, bool giamDan, int trang, int soDong, out int tongSo)
    {
        if (soDong < 1) throw new BusinessException("Số dòng mỗi trang phải lớn hơn 0.");
        if (giaTu.HasValue && giaDen.HasValue && giaTu > giaDen)
            throw new BusinessException("Giá bán từ phải nhỏ hơn hoặc bằng giá bán đến.");

        string cot = CotSapXepHopLe.Contains(sapXep) ? sapXep : "MaSach";
        return _dal.SearchPaged(keyword, maTL, giaTu, giaDen, cot, giamDan, Math.Max(1, trang), soDong, out tongSo);
    }

    public Sach GetById(string maSach) => _dal.GetById(maSach);

    public string SinhMaSachMoi() =>
        FormatHelper.GenerateNextCode("S", _dal.GetLastCode(), 3);

    /// <summary>Kiểm tra sách còn đủ số lượng để bán; ném BusinessException với thông báo rõ ràng nếu không đủ.</summary>
    public void KiemTraTon(string maSach, int soLuongCan)
    {
        Sach s = _dal.GetById(maSach);
        if (s == null || !s.TrangThai)
            throw new BusinessException($"Sách {maSach} không tồn tại hoặc đã ngừng kinh doanh.");
        if (!s.ConHang(soLuongCan))
            throw new BusinessException($"Sách \"{s.TenSach}\" chỉ còn {s.SoLuongTon} cuốn, không đủ {soLuongCan} cuốn.");
    }

    public bool LuuSach(Sach s, bool isNew)
    {
        PhanQuyenBUS.YeuCau(ManHinh.Sach);
        if (!s.IsValid(out string loi)) throw new BusinessException(loi);
        if (s.GiaBan < s.GiaNhap)
            throw new BusinessException("Giá bán phải lớn hơn hoặc bằng giá nhập.");

        bool ok = isNew ? _dal.Insert(s) : _dal.Update(s);
        AuditBUS.Ghi(isNew ? "THEM_SACH" : "SUA_SACH", s.MaSach, s.TenSach);
        return ok;
    }

    public bool NgungKinhDoanh(string maSach)
    {
        PhanQuyenBUS.YeuCau(ManHinh.Sach);
        bool ok = _dal.Delete(maSach);
        AuditBUS.Ghi("NGUNG_KINH_DOANH_SACH", maSach);
        return ok;
    }
}
