namespace QLCuaHangSach.DTO;

/// <summary>
/// Hợp đồng kiểm tra dữ liệu: mọi đối tượng ký hợp đồng này phải tự báo được
/// dữ liệu của mình có hợp lệ hay không và lý do nếu sai. BUS chỉ cần gọi IsValid().
/// </summary>
public interface IValidatable
{
    bool IsValid(out string message);
}
