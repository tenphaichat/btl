using System.Windows.Forms;

namespace QLCuaHangSach.GUI;

/// <summary>
/// Theo dõi người dùng đã sửa dữ liệu trên form hay chưa. Khi form tự nạp dữ liệu vào ô nhập
/// (chọn dòng trên lưới) thì gọi NapDuLieu() để không bị tính là "đã sửa".
/// </summary>
public class TheoDoiThayDoi
{
    private bool _dangNap;

    public bool DaSua { get; private set; }

    /// <summary>Đăng ký theo dõi các ô nhập: bất kỳ thay đổi nào của người dùng sẽ đặt DaSua = true.</summary>
    public void Theo(params Control[] cacOnhap)
    {
        foreach (Control c in cacOnhap)
        {
            switch (c)
            {
                case TextBoxBase t: t.TextChanged += BaoThayDoi; break;
                case NumericUpDown n: n.ValueChanged += BaoThayDoi; break;
                case ComboBox cb: cb.SelectedIndexChanged += BaoThayDoi; break;
                case RadioButton r: r.CheckedChanged += BaoThayDoi; break;
                case CheckBox ch: ch.CheckedChanged += BaoThayDoi; break;
            }
        }
    }

    /// <summary>Chạy đoạn mã nạp dữ liệu vào các ô nhập mà không đánh dấu "đã sửa"; kết thúc coi như chưa có thay đổi.</summary>
    public void NapDuLieu(Action nap)
    {
        _dangNap = true;
        try { nap(); }
        finally { _dangNap = false; }
        DaSua = false;
    }

    public void DanhDauDaLuu() => DaSua = false;

    private void BaoThayDoi(object sender, EventArgs e)
    {
        if (!_dangNap) DaSua = true;
    }
}
