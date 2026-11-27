using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>Giao diện 7: Lập Hóa đơn Bán hàng POS & Tra cứu lịch sử Hóa đơn.</summary>
public class frmBanHang : Form
{
    private readonly SachBUS _sachBus = new SachBUS();
    private readonly KhachHangBUS _khBus = new KhachHangBUS();
    private readonly BanHangBUS _bhBus = new BanHangBUS();

    private HoaDon _currentHoaDon = new HoaDon();
    private KhachHang _khachHang;   // khách đang chọn (null = khách lẻ)
    public TextBox txtMaHD, txtTimSach;
    public ComboBox cboKhachHang;
    public NumericUpDown nudSoLuong;
    public DataGridView dgvChonSach, dgvGioHang, dgvLichSuHD, dgvChiTietHD;
    public Label lblTongTien, lblGiamGia, lblThanhToan;

    public frmBanHang()
    {
        InitializeComponent();

        // Lab 5 (FormClosing): còn hàng trong giỏ chưa thanh toán thì hỏi xác nhận trước khi đóng
        FormClosing += (s, e) =>
        {
            if (e.CloseReason != CloseReason.UserClosing || _currentHoaDon.ChiTiet.Count == 0) return;
            DialogResult kq = MessageBox.Show(this, "Giỏ hàng còn sản phẩm chưa thanh toán. Đóng màn hình bán hàng và bỏ giỏ hàng này?",
                "Xác nhận đóng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (kq == DialogResult.No) e.Cancel = true;
        };
        InitNewOrder();
    }

    private void InitializeComponent()
    {
        Text = "Nghiệp vụ Bán hàng POS & Quản lý Hóa đơn - Nhà sách Đông Á";
        Size = new Size(1180, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UITheme.BgLight;

        var pnlHeader = UITheme.CreateHeaderBanner(
            "QUẦY THU NGÂN BÁN SÁCH (POS) & QUẢN LÝ HÓA ĐƠN",
            "Chọn sách vào giỏ, tự động khấu trừ tồn kho qua Trigger, giảm giá thành viên và cộng điểm tích lũy");

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = UITheme.FontBold };
        var tabPOS = new TabPage("1. Lập Hóa đơn Bán hàng (POS)");
        var tabHistory = new TabPage("2. Lịch sử Hóa đơn đã lập");

        // Left panel: Book Selection
        var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 540, Padding = new Padding(10), BackColor = Color.White };
        var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 44 };
        pnlSearch.Controls.Add(new Label { Text = "Tìm sách:", Location = new Point(6, 12), AutoSize = true, Font = UITheme.FontRegular });
        txtTimSach = new TextBox { Location = new Point(75, 9), Width = 200, Font = UITheme.FontRegular };
        txtTimSach.TextChanged += (s, e) => LoadBooks();
        pnlSearch.Controls.Add(new Label { Text = "SL:", Location = new Point(285, 12), AutoSize = true, Font = UITheme.FontRegular });
        nudSoLuong = new NumericUpDown { Location = new Point(315, 9), Width = 65, Minimum = 1, Maximum = 200, Value = 1, Font = UITheme.FontRegular };
        var btnAddCart = UITheme.CreateButton("+ Thêm vào giỏ", UITheme.Accent, 130, 30);
        btnAddCart.Location = new Point(390, 7);
        btnAddCart.Click += BtnAddCart_Click;
        pnlSearch.Controls.AddRange(new Control[] { txtTimSach, nudSoLuong, btnAddCart });

        dgvChonSach = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvChonSach);
        pnlLeft.Controls.Add(dgvChonSach);
        pnlLeft.Controls.Add(pnlSearch);

        // Right panel: Cart + Checkout
        var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = UITheme.BgLight };
        var grpInfo = new GroupBox { Text = "Thông tin hóa đơn & Thanh toán", Dock = DockStyle.Top, Height = 195, BackColor = Color.White, Font = UITheme.FontBold };

        grpInfo.Controls.Add(new Label { Text = "Mã HĐ:", Location = new Point(16, 30), AutoSize = true, Font = UITheme.FontRegular });
        txtMaHD = new TextBox { Location = new Point(85, 27), Width = 120, ReadOnly = true, Font = UITheme.FontBold };

        grpInfo.Controls.Add(new Label { Text = "Khách hàng:", Location = new Point(220, 30), AutoSize = true, Font = UITheme.FontRegular });
        cboKhachHang = new ComboBox { Location = new Point(310, 27), Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };
        cboKhachHang.SelectedIndexChanged += (s, e) => { CapNhatKhachHang(); RefreshTotals(); };

        lblTongTien = new Label { Text = "Tổng tiền hàng: 0 đ", Location = new Point(16, 68), AutoSize = true, Font = UITheme.FontBold };
        lblGiamGia = new Label { Text = "Giảm giá thành viên: 0 đ", Location = new Point(265, 68), AutoSize = true, Font = UITheme.FontBold, ForeColor = UITheme.Warning };
        lblThanhToan = new Label { Text = "KHÁCH CẦN TRẢ: 0 đ", Location = new Point(16, 104), AutoSize = true, Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = UITheme.Danger };

        var btnThanhToan = UITheme.CreateButton("THANH TOÁN & LƯU HĐ", UITheme.Success, 185, 36);
        btnThanhToan.Location = new Point(310, 96);
        btnThanhToan.Click += BtnThanhToan_Click;

        var btnXoaDong = UITheme.CreateButton("Xóa món", UITheme.Danger, 90, 36);
        btnXoaDong.Location = new Point(505, 96);
        btnXoaDong.Click += (s, e) =>
        {
            if (dgvGioHang.CurrentRow == null) return;
            string ma = dgvGioHang.CurrentRow.Cells["MaSach"].Value?.ToString();
            _currentHoaDon.XoaChiTiet(ma);
            RefreshCartGrid();
        };

        // Xem trước hóa đơn: HoaDon thực thi interface IPrintable nên chỉ cần gọi ToPrintText()
        var btnXemTruoc = UITheme.CreateButton("Xem trước hóa đơn", UITheme.Primary, 170, 32);
        btnXemTruoc.Location = new Point(16, 148);
        btnXemTruoc.Click += BtnXemTruoc_Click;

        grpInfo.Controls.AddRange(new Control[] { txtMaHD, cboKhachHang, lblTongTien, lblGiamGia, lblThanhToan, btnThanhToan, btnXoaDong, btnXemTruoc });

        dgvGioHang = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvGioHang);

        pnlRight.Controls.Add(dgvGioHang);
        pnlRight.Controls.Add(grpInfo);

        tabPOS.Controls.Add(pnlRight);
        tabPOS.Controls.Add(pnlLeft);

        // History Tab
        dgvLichSuHD = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvLichSuHD);
        dgvLichSuHD.SelectionChanged += (s, e) => LoadChiTietHoaDon();   // master -> detail

        dgvChiTietHD = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvChiTietHD);
        var grpChiTiet = new GroupBox
        {
            Text = "Chi tiết hóa đơn đang chọn (master-detail)",
            Dock = DockStyle.Bottom, Height = 230, Font = UITheme.FontBold, BackColor = Color.White
        };
        grpChiTiet.Controls.Add(dgvChiTietHD);

        var pnlInHD = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };
        var btnInHD = UITheme.CreateButton("In hóa đơn đang chọn (Crystal Report)", UITheme.PrimaryDark, 290, 30);
        btnInHD.Location = new Point(8, 7);
        btnInHD.Click += (s, e) =>
        {
            if (dgvLichSuHD.CurrentRow == null) return;
            InHoaDon(dgvLichSuHD.CurrentRow.Cells["MaHD"].Value?.ToString());
        };
        pnlInHD.Controls.Add(btnInHD);

        tabHistory.Controls.Add(dgvLichSuHD);
        tabHistory.Controls.Add(grpChiTiet);
        tabHistory.Controls.Add(pnlInHD);

        tabs.TabPages.AddRange(new[] { tabPOS, tabHistory });

        Controls.Add(tabs);
        Controls.Add(pnlHeader);
    }

    private void LoadBooks()
    {
        DataTable dt = _sachBus.TimKiem(txtTimSach?.Text?.Trim() ?? "");
        dgvChonSach.DataSource = dt;
        string[] hide = { "MaTL", "TenTL", "MaTG", "MaNXB", "TenNXB", "NamXuatBan", "GiaNhap", "MoTa", "TrangThai" };
        foreach (string col in hide)
            if (dgvChonSach.Columns[col] != null) dgvChonSach.Columns[col].Visible = false;
        if (dgvChonSach.Columns["GiaBan"] != null) dgvChonSach.Columns["GiaBan"].DefaultCellStyle.Format = "N0";
    }

    private void InitNewOrder()
    {
        LoadBooks();
        DataTable dtKH = _khBus.GetAll();
        DataRow rLe = dtKH.NewRow();
        rLe["MaKH"] = "";
        rLe["HoTen"] = "-- Khách vãng lai (Khách lẻ) --";
        dtKH.Rows.InsertAt(rLe, 0);
        cboKhachHang.DataSource = dtKH;
        cboKhachHang.DisplayMember = "HoTen";
        cboKhachHang.ValueMember = "MaKH";
        cboKhachHang.SelectedIndex = 0;   // mặc định: khách lẻ
        CapNhatKhachHang();

        _currentHoaDon = new HoaDon
        {
            MaChungTu = _bhBus.SinhMaHoaDonMoi(),
            MaNV = SessionManager.CurrentUser.MaNV,
            TenNV = SessionManager.CurrentUser.HoTen,
            NgayLap = DateTime.Now
        };
        txtMaHD.Text = _currentHoaDon.MaChungTu;
        RefreshCartGrid();   // giỏ hàng bắt đầu rỗng

        dgvLichSuHD.DataSource = _bhBus.GetDanhSachHoaDon();
    }

    private void BtnAddCart_Click(object sender, EventArgs e)
    {
        if (dgvChonSach.CurrentRow == null) return;
        var r = dgvChonSach.CurrentRow;
        try
        {
            string maSach = r.Cells["MaSach"].Value.ToString();
            int soLuong = (int)nudSoLuong.Value;

            // Chặn ngay khi thêm vào giỏ nếu vượt tồn kho (tính cả số lượng đã có trong giỏ)
            _bhBus.KiemTraTonTruocKhiThem(_currentHoaDon, maSach, soLuong);

            _currentHoaDon.ThemChiTiet(new ChiTietHoaDon
            {
                MaSach = maSach,
                TenSach = r.Cells["TenSach"].Value.ToString(),
                SoLuong = soLuong,
                DonGia = Convert.ToDecimal(r.Cells["GiaBan"].Value)
            });
            RefreshCartGrid();
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex, "Không thể thêm vào giỏ");
        }
    }

    private void RefreshCartGrid()
    {
        var dt = new DataTable();
        dt.Columns.Add("MaSach");
        dt.Columns.Add("TenSach");
        dt.Columns.Add("SoLuong", typeof(int));
        dt.Columns.Add("DonGia", typeof(decimal));
        dt.Columns.Add("ThanhTien", typeof(decimal));
        foreach (var c in _currentHoaDon.ChiTiet)
            dt.Rows.Add(c.MaSach, c.TenSach, c.SoLuong, c.DonGia, c.ThanhTien);

        dgvGioHang.DataSource = dt;
        if (dgvGioHang.Columns["DonGia"] != null) dgvGioHang.Columns["DonGia"].DefaultCellStyle.Format = "N0";
        if (dgvGioHang.Columns["ThanhTien"] != null) dgvGioHang.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
        RefreshTotals();
    }

    private void RefreshTotals()
    {
        _currentHoaDon.TinhTongTien();
        _currentHoaDon.MaKH = _khachHang?.MaKH;
        _currentHoaDon.TenKH = _khachHang?.HoTen;
        _currentHoaDon.GiamGia = _bhBus.TinhGiamGia(_currentHoaDon.TongTien, _khachHang);

        // Nhãn giảm giá cho biết rõ lý do: tỷ lệ theo hạng, hay chưa đủ điều kiện
        decimal tyLe = _bhBus.TyLeGiamGia(_currentHoaDon.TongTien, _khachHang);
        string lyDo = _khachHang == null ? "khách lẻ không được giảm"
                    : tyLe > 0 ? $"hạng {_khachHang.HangThanhVien} {tyLe:P0}"
                    : "đơn từ 500.000 đ mới được giảm";

        lblTongTien.Text = $"Tổng tiền hàng: {FormatHelper.FormatCurrency(_currentHoaDon.TongTien)}";
        lblGiamGia.Text = $"Giảm giá ({lyDo}): {FormatHelper.FormatCurrency(_currentHoaDon.GiamGia)}";
        lblThanhToan.Text = $"KHÁCH CẦN TRẢ: {FormatHelper.FormatCurrency(_currentHoaDon.ThanhToan)}";
    }

    private void BtnThanhToan_Click(object sender, EventArgs e)
    {
        try
        {
            _bhBus.ThanhToanHoaDon(_currentHoaDon);
            string maHD = _currentHoaDon.MaChungTu;
            DialogResult inNgay = MessageBox.Show(
                $"Đã thanh toán hóa đơn {maHD} thành công!\nSố tiền: {FormatHelper.FormatCurrency(_currentHoaDon.ThanhToan)}\n\nIn hóa đơn bằng Crystal Report?",
                "Hoàn tất thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            _currentHoaDon.ChiTiet.Clear();
            InitNewOrder();
            if (inNgay == DialogResult.Yes) InHoaDon(maHD);
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex, "Không thể thanh toán");
        }
    }

    /// <summary>In hóa đã lưu trong CSDL bằng báo cáo Crystal Report (HoaDon.rpt).</summary>
    private void InHoaDon(string maHD)
    {
        if (string.IsNullOrWhiteSpace(maHD)) return;
        try
        {
            ReportLauncher.Mo("HoaDon", new BaoCaoBUS().DuLieuHoaDon(maHD));
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex, "Không thể in hóa đơn");
        }
    }

    private void BtnXemTruoc_Click(object sender, EventArgs e)
    {
        if (_currentHoaDon.ChiTiet.Count == 0)
        {
            MessageBox.Show(this, "Giỏ hàng đang trống, chưa có gì để xem trước.", "Xem trước hóa đơn");
            return;
        }
        _currentHoaDon.TinhTongTien();
        using var frm = new frmXemTruoc("Xem trước hóa đơn " + _currentHoaDon.MaChungTu, _currentHoaDon.ToPrintText());
        frm.ShowDialog(this);
    }

    /// <summary>Nạp thông tin khách đang chọn trong ComboBox (null nếu chọn khách lẻ).</summary>
    private void CapNhatKhachHang()
    {
        try
        {
            string maKH = cboKhachHang.SelectedValue?.ToString();
            _khachHang = string.IsNullOrWhiteSpace(maKH) ? null : _bhBus.GetKhachHang(maKH);
        }
        catch (Exception ex)
        {
            _khachHang = null;
            ErrorHandler.Show(this, ex);
        }
    }

    /// <summary>Master-detail: chọn một hóa đơn ở lưới trên thì lưới dưới hiện các dòng chi tiết của hóa đơn đó.</summary>
    private void LoadChiTietHoaDon()
    {
        if (dgvLichSuHD.CurrentRow == null || dgvLichSuHD.Columns["MaHD"] == null)
        {
            dgvChiTietHD.DataSource = null;
            return;
        }
        string maHD = dgvLichSuHD.CurrentRow.Cells["MaHD"].Value?.ToString();
        if (string.IsNullOrEmpty(maHD)) return;
        try
        {
            dgvChiTietHD.DataSource = _bhBus.GetChiTietHoaDon(maHD);
            if (dgvChiTietHD.Columns["DonGia"] != null) dgvChiTietHD.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            if (dgvChiTietHD.Columns["ThanhTien"] != null) dgvChiTietHD.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex);
        }
    }
}
