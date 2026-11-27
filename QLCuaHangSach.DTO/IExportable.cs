using System.Data;

namespace QLCuaHangSach.DTO;

/// <summary>Hợp đồng "có thể xuất dữ liệu": chuyển đối tượng thành DataTable để xuất Excel hoặc làm nguồn cho báo cáo Crystal Report.</summary>
public interface IExportable
{
    DataTable ToDataTable();
}
