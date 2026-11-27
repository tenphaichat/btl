using System.Data;
using QLCuaHangSach.DAL;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.BUS;

/// <summary>Nghiệp vụ danh mục: thể loại, tác giả, nhà xuất bản, nhà cung cấp (đủ thêm/sửa/xóa).</summary>
public class DanhMucBUS
{
    private readonly TheLoaiDAL _theLoai = new TheLoaiDAL();
    private readonly TacGiaDAL _tacGia = new TacGiaDAL();
    private readonly NhaXuatBanDAL _nxb = new NhaXuatBanDAL();
    private readonly NhaCungCapDAL _ncc = new NhaCungCapDAL();

    public DataTable GetTheLoai() => _theLoai.GetAll();
    public DataTable GetTacGia() => _tacGia.GetAll();
    public DataTable GetNhaXuatBan() => _nxb.GetAll();
    public DataTable GetNhaCungCap() => _ncc.GetAll();

    public bool LuuTheLoai(string ma, string ten, string moTa, bool isNew) =>
        Luu(_theLoai, new TheLoai { MaDM = ma?.Trim(), Ten = ten?.Trim(), MoTa = moTa }, isNew, "THE_LOAI");

    public bool LuuTacGia(string ma, string ten, string quocTich, bool isNew) =>
        Luu(_tacGia, new TacGia { MaDM = ma?.Trim(), Ten = ten?.Trim(), QuocTich = quocTich }, isNew, "TAC_GIA");

    public bool LuuNhaXuatBan(string ma, string ten, string diaChi, bool isNew) =>
        Luu(_nxb, new NhaXuatBan { MaDM = ma?.Trim(), Ten = ten?.Trim(), DiaChi = diaChi }, isNew, "NXB");

    public bool LuuNhaCungCap(NhaCungCap ncc, bool isNew) =>
        Luu(_ncc, ncc, isNew, "NCC");

    public bool XoaTheLoai(string ma) => Xoa(_theLoai, ma, "THE_LOAI");
    public bool XoaTacGia(string ma) => Xoa(_tacGia, ma, "TAC_GIA");
    public bool XoaNhaXuatBan(string ma) => Xoa(_nxb, ma, "NXB");
    public bool XoaNhaCungCap(string ma) => Xoa(_ncc, ma, "NCC");

    // Mọi danh mục đều ký hợp đồng IRepository<T> nên dùng chung một đường xử lý.
    private static bool Luu<T>(IRepository<T> repo, T entity, bool isNew, string loai) where T : EntityBase
    {
        PhanQuyenBUS.YeuCau(ManHinh.DanhMuc);
        if (!entity.IsValid(out string loi)) throw new BusinessException(loi);
        bool ok = isNew ? repo.Insert(entity) : repo.Update(entity);
        AuditBUS.Ghi((isNew ? "THEM_" : "SUA_") + loai, entity.Ma, entity.HienThi());
        return ok;
    }

    private static bool Xoa<T>(IRepository<T> repo, string ma, string loai) where T : EntityBase
    {
        PhanQuyenBUS.YeuCau(ManHinh.DanhMuc);
        bool ok = repo.Delete(ma);
        AuditBUS.Ghi("XOA_" + loai, ma);
        return ok;
    }
}
