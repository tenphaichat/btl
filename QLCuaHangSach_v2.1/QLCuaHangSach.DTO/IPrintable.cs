namespace QLCuaHangSach.DTO;

/// <summary>Hợp đồng "có thể in": trả về nội dung văn bản để xem trước hoặc in (hóa đơn, phiếu nhập, thẻ thành viên).</summary>
public interface IPrintable
{
    string ToPrintText();
}
