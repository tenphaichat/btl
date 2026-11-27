using System.Data;
using System.Data.SqlClient;

namespace QLCuaHangSach.DAL;

public class ThongKeDAL
{
    public DataTable GetTongQuan() =>
        DatabaseConnection.ExecuteQuery("sp_ThongKeTongQuan", CommandType.StoredProcedure);

    public DataTable TimHoaDonGiaTriCaoNhat(DateTime? tuNgay = null, DateTime? denNgay = null) =>
        DatabaseConnection.ExecuteQuery("sp_TimHoaDonGiaTriCaoNhat", CommandType.StoredProcedure,
            new SqlParameter("@TuNgay", (object)tuNgay?.Date ?? DBNull.Value),
            new SqlParameter("@DenNgay", (object)denNgay?.Date ?? DBNull.Value));

    public DataTable SachBanChay(DateTime tuNgay, DateTime denNgay, int top = 10) =>
        DatabaseConnection.ExecuteQuery("sp_SachBanChayTheoKhoangThoiGian", CommandType.StoredProcedure,
            new SqlParameter("@TuNgay", tuNgay.Date),
            new SqlParameter("@DenNgay", denNgay.Date),
            new SqlParameter("@Top", top));

    public DataTable DoanhThuTheoThang(int nam) =>
        DatabaseConnection.ExecuteQuery("sp_DoanhThuTheoThang", CommandType.StoredProcedure,
            new SqlParameter("@Nam", nam));

    public DataTable SachSapHetHang(int nguong = 10) =>
        DatabaseConnection.ExecuteQuery("sp_SachSapHetHang", CommandType.StoredProcedure,
            new SqlParameter("@NguongTon", nguong));

    public DataTable TopKhachHang(int top = 10) =>
        DatabaseConnection.ExecuteQuery("sp_TopKhachHang", CommandType.StoredProcedure,
            new SqlParameter("@Top", top));
}
