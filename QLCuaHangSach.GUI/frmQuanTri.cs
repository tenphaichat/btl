using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;

namespace QLCuaHangSach.GUI;

/// <summary>
/// Giao diện Quản trị hệ thống (chỉ Quản lý): nhật ký người dùng, hiệu năng nghiệp vụ, nhật ký lỗi (file log).
/// </summary>
public class frmQuanTri : Form
{
    private readonly AuditBUS _bus = new AuditBUS();

    private DateTimePicker dtpTu, dtpDen, dtpNgayLog;
    private TextBox txtNguoiDung, txtLog;
    private DataGridView dgvNhatKy, dgvHieuNang;

    public frmQuanTri()
    {
        Text = "Quản trị hệ thống - Nhật ký & Hiệu năng";
        Size = new Size(1100, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UITheme.BgLight;

        var pnlHeader = UITheme.CreateHeaderBanner(
            "QUẢN TRỊ HỆ THỐNG",
            "Nhật ký thao tác của người dùng, thống kê hiệu năng nghiệp vụ và nhật ký lỗi của ứng dụng");

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = UITheme.FontBold };
        var tabNhatKy = new TabPage("1. Nhật ký người dùng");
        var tabHieuNang = new TabPage("2. Hiệu năng nghiệp vụ");
        var tabLog = new TabPage("3. Nhật ký lỗi (file log)");

        // ---- Tab 1: nhật ký người dùng
        var pnlLoc = new Panel { Dock = DockStyle.Top, Height = 46 };
        pnlLoc.Controls.Add(new Label { Text = "Từ ngày:", Location = new Point(8, 14), AutoSize = true, Font = UITheme.FontRegular });
        dtpTu = new DateTimePicker { Location = new Point(70, 10), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-7) };
        pnlLoc.Controls.Add(new Label { Text = "Đến ngày:", Location = new Point(195, 14), AutoSize = true, Font = UITheme.FontRegular });
        dtpDen = new DateTimePicker { Location = new Point(265, 10), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        pnlLoc.Controls.Add(new Label { Text = "Người dùng:", Location = new Point(392, 14), AutoSize = true, Font = UITheme.FontRegular });
        txtNguoiDung = new TextBox { Location = new Point(470, 10), Width = 130, Font = UITheme.FontRegular };
        var btnXem = UITheme.CreateButton("Xem nhật ký", UITheme.Accent, 120, 30);
        btnXem.Location = new Point(615, 8);
        btnXem.Click += (s, e) => TaiNhatKy();
        pnlLoc.Controls.AddRange(new Control[] { dtpTu, dtpDen, txtNguoiDung, btnXem });

        dgvNhatKy = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvNhatKy);
        tabNhatKy.Controls.Add(dgvNhatKy);
        tabNhatKy.Controls.Add(pnlLoc);

        // ---- Tab 2: hiệu năng
        var pnlHn = new Panel { Dock = DockStyle.Top, Height = 46 };
        var btnLamMoi = UITheme.CreateButton("Làm mới", UITheme.Accent, 100, 30);
        btnLamMoi.Location = new Point(8, 8);
        btnLamMoi.Click += (s, e) => TaiHieuNang();
        pnlHn.Controls.Add(btnLamMoi);
        pnlHn.Controls.Add(new Label
        {
            Text = "Thời gian xử lý (mili-giây) của các nghiệp vụ chính: đăng nhập, thanh toán, nhập kho.",
            Location = new Point(120, 14), AutoSize = true, Font = UITheme.FontRegular
        });
        dgvHieuNang = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvHieuNang);
        tabHieuNang.Controls.Add(dgvHieuNang);
        tabHieuNang.Controls.Add(pnlHn);

        // ---- Tab 3: file log
        var pnlLog = new Panel { Dock = DockStyle.Top, Height = 46 };
        pnlLog.Controls.Add(new Label { Text = "Ngày:", Location = new Point(8, 14), AutoSize = true, Font = UITheme.FontRegular });
        dtpNgayLog = new DateTimePicker { Location = new Point(55, 10), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        var btnDocLog = UITheme.CreateButton("Xem log", UITheme.Accent, 100, 30);
        btnDocLog.Location = new Point(180, 8);
        btnDocLog.Click += (s, e) => TaiLog();
        var btnMoThuMuc = UITheme.CreateButton("Mở thư mục Logs", UITheme.PrimaryDark, 150, 30);
        btnMoThuMuc.Location = new Point(290, 8);
        btnMoThuMuc.Click += (s, e) => MoThuMucLog();
        pnlLog.Controls.AddRange(new Control[] { dtpNgayLog, btnDocLog, btnMoThuMuc });

        txtLog = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both,
            WordWrap = false, Font = new Font("Consolas", 9.5F), BackColor = Color.White
        };
        tabLog.Controls.Add(txtLog);
        tabLog.Controls.Add(pnlLog);

        tabs.TabPages.AddRange(new[] { tabNhatKy, tabHieuNang, tabLog });
        tabs.SelectedIndexChanged += (s, e) =>
        {
            if (tabs.SelectedTab == tabHieuNang) TaiHieuNang();
            else if (tabs.SelectedTab == tabLog) TaiLog();
        };

        Controls.Add(tabs);
        Controls.Add(pnlHeader);

        Shown += (s, e) => TaiNhatKy();
    }

    private void TaiNhatKy()
    {
        try
        {
            dgvNhatKy.DataSource = _bus.TimKiem(dtpTu.Value, dtpDen.Value, txtNguoiDung.Text);
            if (dgvNhatKy.Columns["ThoiGian"] != null) dgvNhatKy.Columns["ThoiGian"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }
        catch (Exception ex) { ErrorHandler.Show(this, ex); }
    }

    private void TaiHieuNang()
    {
        try { dgvHieuNang.DataSource = _bus.HieuNang(); }
        catch (Exception ex) { ErrorHandler.Show(this, ex); }
    }

    private void TaiLog()
    {
        try { txtLog.Text = _bus.DocFileLog(dtpNgayLog.Value); }
        catch (Exception ex) { ErrorHandler.Show(this, ex); }
    }

    private void MoThuMucLog()
    {
        try
        {
            string dir = _bus.ThuMucLog;
            if (Directory.Exists(dir)) Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
            else MessageBox.Show(this, "Chưa có thư mục log (chưa có lỗi nào được ghi).", "Thông báo");
        }
        catch (Exception ex) { ErrorHandler.Show(this, ex); }
    }
}
