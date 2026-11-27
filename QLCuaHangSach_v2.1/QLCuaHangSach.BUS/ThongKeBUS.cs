using System.Data;
using QLCuaHangSach.DAL;

namespace QLCuaHangSach.BUS;

public class ThongKeBUS
{
    private readonly ThongKeDAL _dal = new ThongKeDAL();

    public DataRow GetTongQuan()
    {
        DataTable dt = _dal.GetTongQuan();
        return dt.Rows.Count > 0 ? dt.Rows[0] : null;
    }

    public DataTable TimHoaDonGiaTriCaoNhat(DateTime? tu = null, DateTime? den = null) =>
        _dal.TimHoaDonGiaTriCaoNhat(tu, den);

    public DataTable SachBanChay(DateTime tu, DateTime den, int top = 10) =>
        _dal.SachBanChay(tu, den, top);

    public DataTable DoanhThuTheoThang(int nam) =>
        _dal.DoanhThuTheoThang(nam);

    public DataTable SachSapHetHang(int nguong = 10) =>
        _dal.SachSapHetHang(nguong);

    public DataTable TopKhachHang(int top = 10) =>
        _dal.TopKhachHang(top);
}
