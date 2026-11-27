using System.Drawing;
using System.Windows.Forms;

namespace QLCuaHangSach.GUI;

/// <summary>Bộ định dạng giao diện thống nhất cho toàn bộ các Form trong hệ thống.</summary>
public static class UITheme
{
    public static readonly Color Primary = Color.FromArgb(30, 58, 138);       // Navy #1E3A8A
    public static readonly Color PrimaryDark = Color.FromArgb(15, 23, 42);    // Slate #0F172A
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);        // Blue #2563EB
    public static readonly Color Success = Color.FromArgb(5, 150, 105);       // Emerald #059669
    public static readonly Color Warning = Color.FromArgb(217, 119, 6);       // Amber #D97706
    public static readonly Color Danger = Color.FromArgb(220, 38, 38);        // Red #DC2626
    public static readonly Color BgLight = Color.FromArgb(241, 245, 249);     // Slate-100
    public static readonly Color CardBg = Color.White;

    public static readonly Font FontTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
    public static readonly Font FontSection = new Font("Segoe UI", 10.5F, FontStyle.Bold);
    public static readonly Font FontRegular = new Font("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font FontBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);

    public static Button CreateButton(string text, Color bgColor, int width = 120, int height = 34)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = FontBold,
            Size = new Size(width, height),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public static void StyleDataGridView(DataGridView dgv)
    {
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = Color.FromArgb(226, 232, 240);
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = FontBold;
        dgv.ColumnHeadersHeight = 34;
        dgv.DefaultCellStyle.Font = FontRegular;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgv.RowTemplate.Height = 28;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowHeadersVisible = false;
    }

    public static Panel CreateHeaderBanner(string title, string subtitle)
    {
        var pnl = new Panel
        {
            Dock = DockStyle.Top,
            Height = 62,
            BackColor = Primary
        };
        var lblT = new Label
        {
            Text = title,
            Font = FontTitle,
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(18, 8)
        };
        var lblS = new Label
        {
            Text = subtitle,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Color.FromArgb(219, 234, 254),
            AutoSize = true,
            Location = new Point(20, 36)
        };
        pnl.Controls.Add(lblT);
        pnl.Controls.Add(lblS);
        return pnl;
    }
}
