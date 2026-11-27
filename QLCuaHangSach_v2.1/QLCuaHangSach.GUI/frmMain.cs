using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>Giao diện 2: Màn hình chính (Dashboard tổng quan, Menu và điều hướng).</summary>
public class frmMain : Form
{
    private readonly ThongKeBUS _tkBus = new ThongKeBUS();
    private MenuStrip _menuStrip;
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _lblStatus;
    private ToolStripStatusLabel _lblUser;
    private Panel _pnlSidebar;
    private Panel _pnlDashboard;
    private Panel _pnlChart;
    private DataGridView _dgvLowStock;
    private DataTable _dtMonthly;

    /// <summary>true nếu người dùng chọn "Đăng xuất" (Program sẽ quay lại màn hình đăng nhập).</summary>
    public bool DaDangXuat { get; private set; }

    private Label _lblKpiToday;
    private Label _lblKpiMonth;
    private Label _lblKpiBooks;
    private Label _lblKpiCustomers;

    public frmMain()
    {
        InitializeComponent();
        LoadDashboardData();

        // Lab 5 (FormClosing): hỏi xác nhận khi thoát (đăng xuất thì không hỏi)
        FormClosing += (s, e) =>
        {
            if (e.CloseReason != CloseReason.UserClosing || DaDangXuat) return;
            DialogResult kq = MessageBox.Show(this, "Bạn có chắc muốn thoát ứng dụng?", "Xác nhận thoát",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (kq == DialogResult.No) e.Cancel = true;
        };

        // Đóng form (đăng xuất hoặc thoát) đều ghi nhật ký và xóa phiên đăng nhập
        FormClosed += (s, e) => { if (SessionManager.IsLoggedIn) new AuthBUS().Logout(); };
    }

    private void InitializeComponent()
    {
        Text = "Phần mềm Quản lý Cửa hàng Sách Đông Á - Đề tài 20 (.NET 9 WinForms)";
        Size = new Size(1240, 740);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UITheme.BgLight;

        // 1. MenuStrip - chỉ hiện những mục mà vai trò hiện tại được phép dùng
        _menuStrip = new MenuStrip { BackColor = Color.White, Font = UITheme.FontRegular };

        var mnuHeThong = new ToolStripMenuItem("Hệ thống");
        mnuHeThong.DropDownItems.Add("Làm mới Dashboard", null, (s, e) => LoadDashboardData());
        if (PhanQuyenBUS.CoQuyen(ManHinh.QuanTri))
            mnuHeThong.DropDownItems.Add("Quản trị: Nhật ký & Hiệu năng", null, (s, e) => OpenDialog(ManHinh.QuanTri, () => new frmQuanTri()));
        mnuHeThong.DropDownItems.Add(new ToolStripSeparator());
        mnuHeThong.DropDownItems.Add("Đăng xuất", null, (s, e) => { DaDangXuat = true; Close(); });
        mnuHeThong.DropDownItems.Add("Thoát", null, (s, e) => Close());

        var mnuDanhMuc = new ToolStripMenuItem("Quản lý Danh mục");
        ThemMucMenu(mnuDanhMuc, "Quản lý Sách", ManHinh.Sach, () => new frmSach());
        ThemMucMenu(mnuDanhMuc, "Thể loại / Tác giả / NXB / NCC", ManHinh.DanhMuc, () => new frmDanhMuc());
        ThemMucMenu(mnuDanhMuc, "Quản lý Khách hàng", ManHinh.KhachHang, () => new frmKhachHang());
        ThemMucMenu(mnuDanhMuc, "Quản lý Nhân viên", ManHinh.NhanVien, () => new frmNhanVien());

        var mnuNghiepVu = new ToolStripMenuItem("Nghiệp vụ Kinh doanh");
        ThemMucMenu(mnuNghiepVu, "Lập Hóa đơn Bán hàng (POS)", ManHinh.BanHang, () => new frmBanHang());
        ThemMucMenu(mnuNghiepVu, "Lập Phiếu Nhập kho Sách", ManHinh.NhapHang, () => new frmNhapHang());

        var mnuBaoCao = new ToolStripMenuItem("Báo cáo & Thống kê");
        ThemMucMenu(mnuBaoCao, "Trung tâm Thống kê & Truy vấn nâng cao", ManHinh.ThongKe, () => new frmThongKe());

        _menuStrip.Items.Add(mnuHeThong);
        foreach (ToolStripMenuItem mnu in new[] { mnuDanhMuc, mnuNghiepVu, mnuBaoCao })
            if (mnu.DropDownItems.Count > 0) _menuStrip.Items.Add(mnu);   // ẩn cả nhóm nếu không có mục nào được phép

        // 2. StatusStrip
        _statusStrip = new StatusStrip { BackColor = Color.White };
        var nguoiDung = SessionManager.CurrentUser;
        string userText = $"Nhân viên: {nguoiDung.HoTen} ({nguoiDung.VaiTroText})";
        _lblStatus = new ToolStripStatusLabel("Hệ thống sẵn sàng") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        _lblUser = new ToolStripStatusLabel(userText);
        _statusStrip.Items.AddRange(new ToolStripItem[] { _lblStatus, _lblUser });

        // 3. Left Sidebar Navigation
        _pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 235,
            BackColor = UITheme.PrimaryDark,
            Padding = new Padding(12)
        };
        var lblAppLogo = new Label
        {
            Text = "BOOKSTORE ERP\nĐỀ TÀI 20 - EAUT",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(16, 16),
            Size = new Size(205, 54)
        };
        _pnlSidebar.Controls.Add(lblAppLogo);

        // Mỗi mục điều hướng gắn với một màn hình; mục nào vai trò hiện tại không được dùng thì không hiển thị.
        var muc = new List<(string Ten, ManHinh ManHinh, Action Mo)>
        {
            ("Trang chủ Dashboard", ManHinh.Dashboard, LoadDashboardData),
            ("Quản lý Sách", ManHinh.Sach, () => OpenDialog(ManHinh.Sach, () => new frmSach())),
            ("Thể loại / Tác giả / NXB", ManHinh.DanhMuc, () => OpenDialog(ManHinh.DanhMuc, () => new frmDanhMuc())),
            ("Bán hàng & Hóa đơn", ManHinh.BanHang, () => OpenDialog(ManHinh.BanHang, () => new frmBanHang())),
            ("Nhập kho Sách", ManHinh.NhapHang, () => OpenDialog(ManHinh.NhapHang, () => new frmNhapHang())),
            ("Khách hàng thân thiết", ManHinh.KhachHang, () => OpenDialog(ManHinh.KhachHang, () => new frmKhachHang())),
            ("Quản lý Nhân viên", ManHinh.NhanVien, () => OpenDialog(ManHinh.NhanVien, () => new frmNhanVien())),
            ("Báo cáo & Thống kê", ManHinh.ThongKe, () => OpenDialog(ManHinh.ThongKe, () => new frmThongKe())),
            ("Quản trị hệ thống", ManHinh.QuanTri, () => OpenDialog(ManHinh.QuanTri, () => new frmQuanTri()))
        };

        int y = 85;
        int stt = 0;
        foreach (var (ten, manHinh, mo) in muc)
        {
            if (!PhanQuyenBUS.CoQuyen(manHinh)) continue;
            stt++;
            var btn = UITheme.CreateButton($"{stt}. {ten}", stt == 1 ? UITheme.Accent : Color.FromArgb(30, 41, 59), 210, 42);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(10, 0, 0, 0);
            btn.Location = new Point(12, y);
            Action hanhDong = mo;
            btn.Click += (s, e) => hanhDong();
            _pnlSidebar.Controls.Add(btn);
            y += 50;
        }

        // 4. Main Dashboard Content
        _pnlDashboard = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };
        var pnlHeader = UITheme.CreateHeaderBanner(
            "BẢNG ĐIỀU KHIỂN TỔNG QUAN CỬA HÀNG SÁCH ĐÔNG Á",
            "Theo dõi doanh thu thời gian thực, tồn kho sách và cảnh báo nhập hàng");

        // KPI Cards
        var pnlKpis = new Panel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(0, 12, 0, 8) };
        pnlKpis.Controls.Add(CreateKpiCard("DOANH THU HÔM NAY", out _lblKpiToday, UITheme.Success, 0));
        pnlKpis.Controls.Add(CreateKpiCard("DOANH THU THÁNG NÀY", out _lblKpiMonth, UITheme.Accent, 245));
        pnlKpis.Controls.Add(CreateKpiCard("ĐẦU SÁCH / TỒN KHO", out _lblKpiBooks, UITheme.Warning, 490));
        pnlKpis.Controls.Add(CreateKpiCard("KHÁCH HÀNG THÀNH VIÊN", out _lblKpiCustomers, UITheme.Primary, 735));

        // Center Area: Left = Monthly Bar Chart, Right = Low Stock Grid
        var pnlCenter = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

        _pnlChart = new Panel
        {
            Location = new Point(0, 8),
            Size = new Size(575, 425),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        _pnlChart.Paint += PnlChart_Paint;

        var pnlLowStock = new Panel
        {
            Location = new Point(590, 8),
            Size = new Size(380, 425),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };
        var lblLowTitle = new Label
        {
            Text = "CẢNH BÁO SÁCH SẮP HẾT HÀNG (TỒN <= 10)",
            Dock = DockStyle.Top,
            Height = 30,
            Font = UITheme.FontBold,
            ForeColor = UITheme.Danger
        };
        _dgvLowStock = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(_dgvLowStock);
        pnlLowStock.Controls.Add(_dgvLowStock);
        pnlLowStock.Controls.Add(lblLowTitle);

        pnlCenter.Controls.Add(_pnlChart);
        pnlCenter.Controls.Add(pnlLowStock);

        _pnlDashboard.Controls.Add(pnlCenter);
        _pnlDashboard.Controls.Add(pnlKpis);
        _pnlDashboard.Controls.Add(pnlHeader);

        Controls.Add(_pnlDashboard);
        Controls.Add(_pnlSidebar);
        Controls.Add(_statusStrip);
        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;
    }

    private Panel CreateKpiCard(string title, out Label lblVal, Color barColor, int x)
    {
        var card = new Panel
        {
            Location = new Point(x, 10),
            Size = new Size(232, 88),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        var stripe = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = barColor };
        var lblT = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(16, 12),
            AutoSize = true
        };
        lblVal = new Label
        {
            Text = "0",
            Font = new Font("Segoe UI", 14.5F, FontStyle.Bold),
            ForeColor = barColor,
            Location = new Point(16, 38),
            AutoSize = true
        };
        card.Controls.Add(lblT);
        card.Controls.Add(lblVal);
        card.Controls.Add(stripe);
        return card;
    }

    /// <summary>Thêm một mục vào menu nếu vai trò hiện tại có quyền với màn hình đó.</summary>
    private void ThemMucMenu(ToolStripMenuItem cha, string tieuDe, ManHinh manHinh, Func<Form> taoForm)
    {
        if (!PhanQuyenBUS.CoQuyen(manHinh)) return;
        cha.DropDownItems.Add(tieuDe, null, (s, e) => OpenDialog(manHinh, taoForm));
    }

    /// <summary>Mở một màn hình: kiểm tra quyền ở BUS trước, mọi lỗi được xử lý tập trung.</summary>
    private void OpenDialog(ManHinh manHinh, Func<Form> taoForm)
    {
        try
        {
            PhanQuyenBUS.YeuCau(manHinh);
            using Form frm = taoForm();
            frm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex);
        }
        LoadDashboardData();
    }

    public void LoadDashboardData()
    {
        try
        {
            DataRow r = _tkBus.GetTongQuan();
            if (r != null)
            {
                _lblKpiToday.Text = FormatHelper.FormatCurrency(Convert.ToDecimal(r["DoanhThuHomNay"]));
                _lblKpiMonth.Text = $"{FormatHelper.FormatCurrency(Convert.ToDecimal(r["DoanhThuThangNay"]))} ({r["SoHoaDonThangNay"]} HĐ)";
                _lblKpiBooks.Text = $"{r["SoDauSach"]} đầu sách / {r["TongTonKho"]} cuốn";
                _lblKpiCustomers.Text = $"{r["SoKhachHang"]} KH ({r["SoSachSapHet"]} sắp hết)";
            }

            _dtMonthly = _tkBus.DoanhThuTheoThang(2026);
            _pnlChart.Invalidate();

            _dgvLowStock.DataSource = _tkBus.SachSapHetHang(30);
            if (_dgvLowStock.Columns["MaSach"] != null) _dgvLowStock.Columns["MaSach"].HeaderText = "Mã";
            if (_dgvLowStock.Columns["TenSach"] != null) _dgvLowStock.Columns["TenSach"].HeaderText = "Tên sách";
            if (_dgvLowStock.Columns["SoLuongTon"] != null) _dgvLowStock.Columns["SoLuongTon"].HeaderText = "Tồn";
            if (_dgvLowStock.Columns["TheLoai"] != null) _dgvLowStock.Columns["TheLoai"].Visible = false;
            if (_dgvLowStock.Columns["NhaXuatBan"] != null) _dgvLowStock.Columns["NhaXuatBan"].Visible = false;
            if (_dgvLowStock.Columns["GiaBan"] != null) _dgvLowStock.Columns["GiaBan"].HeaderText = "Giá bán";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi tải Dashboard: " + ErrorHandler.Message(ex);
        }
    }

    private void PnlChart_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        using var fTitle = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        g.DrawString("BIỂU ĐỒ DOANH THU & LỢI NHUẬN THEO THÁNG NĂM 2026 (Triệu VNĐ)",
            fTitle, new SolidBrush(UITheme.Primary), 16, 12);

        if (_dtMonthly == null || _dtMonthly.Rows.Count == 0) return;

        int left = 55, top = 50, right = _pnlChart.Width - 20, bottom = _pnlChart.Height - 40;
        int plotW = right - left, plotH = bottom - top;

        decimal maxVal = 1m;
        foreach (DataRow row in _dtMonthly.Rows)
        {
            decimal dt = Convert.ToDecimal(row["DoanhThu"]) / 1_000_000m;
            if (dt > maxVal) maxVal = dt;
        }
        maxVal = Math.Ceiling(maxVal * 1.15m);
        if (maxVal < 5m) maxVal = 5m;

        using var penGrid = new Pen(Color.FromArgb(226, 232, 240), 1f);
        using var fAxis = new Font("Segoe UI", 8F);
        for (int i = 0; i <= 4; i++)
        {
            int y = bottom - (plotH * i / 4);
            decimal val = maxVal * i / 4;
            g.DrawLine(penGrid, left, y, right, y);
            g.DrawString($"{val:N1}M", fAxis, Brushes.DimGray, 8, y - 7);
        }

        int count = _dtMonthly.Rows.Count;
        int slotW = plotW / count;
        int barW = Math.Max(14, slotW - 16);

        using var brRev = new SolidBrush(UITheme.Accent);
        using var brPro = new SolidBrush(UITheme.Success);

        for (int i = 0; i < count; i++)
        {
            DataRow r = _dtMonthly.Rows[i];
            decimal rev = Convert.ToDecimal(r["DoanhThu"]) / 1_000_000m;
            decimal pro = Convert.ToDecimal(r["LoiNhuan"]) / 1_000_000m;

            int hRev = (int)((double)(rev / maxVal) * plotH);
            int hPro = (int)((double)(pro / maxVal) * plotH);

            int x = left + i * slotW + 8;
            g.FillRectangle(brRev, x, bottom - hRev, barW / 2, hRev);
            g.FillRectangle(brPro, x + barW / 2 + 1, bottom - hPro, barW / 2, hPro);

            g.DrawString("T" + r["Thang"], fAxis, Brushes.Black, x + 2, bottom + 6);
        }
    }
}
