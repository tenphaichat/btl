using System.Windows.Forms;

namespace QLCuaHangSach.GUI;

/// <summary>Tiện ích nhập liệu dùng chung cho các form (Lab 5): giới hạn ký tự, phím Enter, xác nhận khi đóng.</summary>
public static class InputHelper
{
    /// <summary>Sự kiện KeyPress: ô chỉ nhận chữ số (vẫn cho phép Backspace và các phím điều khiển).</summary>
    public static void ChiChoNhapChuSo(TextBox txt)
    {
        txt.KeyPress += (s, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        };
    }

    /// <summary>Sự kiện KeyPress: nhấn Enter trong ô thì chạy hành động (ví dụ tìm kiếm).</summary>
    public static void EnterDeThucHien(TextBox txt, Action hanhDong)
    {
        txt.KeyPress += (s, e) =>
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            e.Handled = true;
            hanhDong();
        };
    }

    /// <summary>Sự kiện FormClosing: nếu còn thay đổi chưa lưu thì hỏi xác nhận, chọn No sẽ hủy việc đóng form.</summary>
    public static void XacNhanKhiDongNeuChuaLuu(Form form, Func<bool> coThayDoi)
    {
        form.FormClosing += (s, e) =>
        {
            if (e.CloseReason != CloseReason.UserClosing || !coThayDoi()) return;
            DialogResult kq = MessageBox.Show(form, "Có thay đổi chưa lưu. Bạn vẫn muốn đóng cửa sổ?",
                "Xác nhận đóng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (kq == DialogResult.No) e.Cancel = true;
        };
    }
}
