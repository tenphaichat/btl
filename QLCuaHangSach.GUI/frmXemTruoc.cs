using System.Drawing;
using System.Windows.Forms;

namespace QLCuaHangSach.GUI;

/// <summary>Hộp thoại xem trước nội dung của đối tượng IPrintable (hóa đơn, phiếu nhập, thẻ thành viên).</summary>
public class frmXemTruoc : Form
{
    public frmXemTruoc(string tieuDe, string noiDung)
    {
        Text = tieuDe;
        Size = new Size(560, 520);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UITheme.BgLight;

        var txt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 10F),
            BackColor = Color.White,
            Text = noiDung
        };
        txt.Select(0, 0);

        var pnlNut = new Panel { Dock = DockStyle.Bottom, Height = 48 };
        var btnCopy = UITheme.CreateButton("Sao chép", UITheme.Accent, 110, 32);
        btnCopy.Location = new Point(12, 8);
        btnCopy.Click += (s, e) => Clipboard.SetText(noiDung);
        var btnDong = UITheme.CreateButton("Đóng", UITheme.PrimaryDark, 100, 32);
        btnDong.Location = new Point(132, 8);
        btnDong.Click += (s, e) => Close();
        pnlNut.Controls.AddRange(new Control[] { btnCopy, btnDong });

        Controls.Add(txt);
        Controls.Add(pnlNut);
    }
}
