using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI
{
    /// <summary>Giao diện 7: Lập Hóa đơn Bán hàng POS & Tra cứu lịch sử Hóa đơn.</summary>
    public class frmBanHang : Form
    {
        private readonly SachBUS _sachBus = new SachBUS();
        private readonly KhachHangBUS _khBus = new KhachHangBUS();
        private readonly BanHangBUS _bhBus = new BanHangBUS();

        private HoaDon _currentHoaDon = new HoaDon();
        public TextBox txtMaHD, txtTimSach;
        public ComboBox cboKhachHang;
        public NumericUpDown nudSoLuong;
        public DataGridView dgvChonSach, dgvGioHang, dgvLichSuHD;
        public Label lblTongTien, lblGiamGia, lblThanhToan;

        public frmBanHang()
        {
            InitializeComponent();
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
            nudSoLuong = new NumericUpDown { Location = new Point(315, 9), Width = 65, Minimum = 1, Maximum = 200, Value = 2, Font = UITheme.FontRegular };
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
            var grpInfo = new GroupBox { Text = "Thông tin hóa đơn & Thanh toán", Dock = DockStyle.Top, Height = 165, BackColor = Color.White, Font = UITheme.FontBold };

            grpInfo.Controls.Add(new Label { Text = "Mã HĐ:", Location = new Point(16, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtMaHD = new TextBox { Location = new Point(85, 27), Width = 120, ReadOnly = true, Font = UITheme.FontBold };

            grpInfo.Controls.Add(new Label { Text = "Khách hàng:", Location = new Point(220, 30), AutoSize = true, Font = UITheme.FontRegular });
            cboKhachHang = new ComboBox { Location = new Point(310, 27), Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };
            cboKhachHang.SelectedIndexChanged += (s, e) => RefreshTotals();

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

            grpInfo.Controls.AddRange(new Control[] { txtMaHD, cboKhachHang, lblTongTien, lblGiamGia, lblThanhToan, btnThanhToan, btnXoaDong });

            dgvGioHang = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvGioHang);

            pnlRight.Controls.Add(dgvGioHang);
            pnlRight.Controls.Add(grpInfo);

            tabPOS.Controls.Add(pnlRight);
            tabPOS.Controls.Add(pnlLeft);

            // History Tab
            dgvLichSuHD = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvLichSuHD);
            tabHistory.Controls.Add(dgvLichSuHD);

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
            cboKhachHang.SelectedIndex = 1;

            _currentHoaDon = new HoaDon
            {
                MaChungTu = _bhBus.SinhMaHoaDonMoi(),
                MaNV = SessionManager.CurrentUser?.MaNV ?? "NV01",
                NgayLap = DateTime.Now
            };
            txtMaHD.Text = _currentHoaDon.MaChungTu;

            // Nạp sẵn 3 mặt hàng mẫu để màn hình POS hiển thị trực quan ngay khi mở
            _currentHoaDon.ThemChiTiet(new ChiTietHoaDon { MaSach = "S008", TenSach = "Clean Code", SoLuong = 1, DonGia = 350000 });
            _currentHoaDon.ThemChiTiet(new ChiTietHoaDon { MaSach = "S001", TenSach = "Mắt biếc", SoLuong = 2, DonGia = 110000 });
            _currentHoaDon.ThemChiTiet(new ChiTietHoaDon { MaSach = "S007", TenSach = "Đắc nhân tâm", SoLuong = 1, DonGia = 86000 });
            RefreshCartGrid();

            dgvLichSuHD.DataSource = _bhBus.GetDanhSachHoaDon();
        }

        private void BtnAddCart_Click(object sender, EventArgs e)
        {
            if (dgvChonSach.CurrentRow == null) return;
            var r = dgvChonSach.CurrentRow;
            _currentHoaDon.ThemChiTiet(new ChiTietHoaDon
            {
                MaSach = r.Cells["MaSach"].Value.ToString(),
                TenSach = r.Cells["TenSach"].Value.ToString(),
                SoLuong = (int)nudSoLuong.Value,
                DonGia = Convert.ToDecimal(r.Cells["GiaBan"].Value)
            });
            RefreshCartGrid();
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
            string maKH = cboKhachHang.SelectedValue?.ToString();
            _currentHoaDon.MaKH = maKH;
            _currentHoaDon.GiamGia = _bhBus.TinhGiamGia(_currentHoaDon.TongTien, maKH);

            lblTongTien.Text = $"Tổng tiền hàng: {FormatHelper.FormatCurrency(_currentHoaDon.TongTien)}";
            lblGiamGia.Text = $"Giảm giá (5%): {FormatHelper.FormatCurrency(_currentHoaDon.GiamGia)}";
            lblThanhToan.Text = $"KHÁCH CẦN TRẢ: {FormatHelper.FormatCurrency(_currentHoaDon.ThanhToan)}";
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            try
            {
                _bhBus.ThanhToanHoaDon(_currentHoaDon);
                MessageBox.Show($"Đã thanh toán hóa đơn {_currentHoaDon.MaChungTu} thành công!\nSố tiền: {FormatHelper.FormatCurrency(_currentHoaDon.ThanhToan)}",
                    "Hoàn tất thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _currentHoaDon.ChiTiet.Clear();
                InitNewOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    /// <summary>Giao diện 8: Quản lý Nhập kho Sách từ Nhà cung cấp (Master-Detail).</summary>
    public class frmNhapHang : Form
    {
        private readonly NhapHangBUS _nhBus = new NhapHangBUS();
        private readonly DanhMucBUS _dmBus = new DanhMucBUS();
        private readonly SachBUS _sachBus = new SachBUS();

        public DataGridView dgvPhieuNhap, dgvChiTietPN;
        public ComboBox cboNCC, cboSach;
        public NumericUpDown nudSoLuong, nudGiaNhap;
        public TextBox txtMaPN, txtGhiChu;

        public frmNhapHang()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Nhập kho Sách từ Nhà cung cấp";
            Size = new Size(1100, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UITheme.BgLight;

            var pnlHeader = UITheme.CreateHeaderBanner(
                "QUẢN LÝ PHIẾU NHẬP KHO SÁCH (MASTER - DETAIL)",
                "Tự động cộng số lượng tồn kho và cập nhật đơn giá nhập mới nhất qua Trigger SQL");

            var grpNew = new GroupBox
            {
                Text = "Lập nhanh phiếu nhập bổ sung kho",
                Font = UITheme.FontBold,
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.White
            };

            grpNew.Controls.Add(new Label { Text = "Mã PN:", Location = new Point(16, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtMaPN = new TextBox { Location = new Point(75, 27), Width = 95, ReadOnly = true, Font = UITheme.FontBold };

            grpNew.Controls.Add(new Label { Text = "Nhà CC:", Location = new Point(185, 30), AutoSize = true, Font = UITheme.FontRegular });
            cboNCC = new ComboBox { Location = new Point(250, 27), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

            grpNew.Controls.Add(new Label { Text = "Đầu sách:", Location = new Point(505, 30), AutoSize = true, Font = UITheme.FontRegular });
            cboSach = new ComboBox { Location = new Point(575, 27), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

            grpNew.Controls.Add(new Label { Text = "SL nhập:", Location = new Point(16, 68), AutoSize = true, Font = UITheme.FontRegular });
            nudSoLuong = new NumericUpDown { Location = new Point(80, 65), Width = 85, Minimum = 1, Maximum = 5000, Value = 25, Font = UITheme.FontRegular };

            grpNew.Controls.Add(new Label { Text = "Đơn giá nhập:", Location = new Point(185, 68), AutoSize = true, Font = UITheme.FontRegular });
            nudGiaNhap = new NumericUpDown { Location = new Point(285, 65), Width = 140, Maximum = 50000000, Increment = 1000, ThousandsSeparator = true, Value = 65000, Font = UITheme.FontRegular };

            grpNew.Controls.Add(new Label { Text = "Ghi chú:", Location = new Point(445, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtGhiChu = new TextBox { Location = new Point(510, 65), Width = 325, Text = "Nhập bổ sung sách bán chạy", Font = UITheme.FontRegular };

            var btnNhapKho = UITheme.CreateButton("Lưu Phiếu Nhập", UITheme.Success, 160, 34);
            btnNhapKho.Location = new Point(855, 60);
            btnNhapKho.Click += BtnNhapKho_Click;

            grpNew.Controls.AddRange(new Control[] { txtMaPN, cboNCC, cboSach, nudSoLuong, nudGiaNhap, txtGhiChu, btnNhapKho });

            var pnlGrids = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            dgvPhieuNhap = new DataGridView { Dock = DockStyle.Top, Height = 230 };
            UITheme.StyleDataGridView(dgvPhieuNhap);
            dgvPhieuNhap.SelectionChanged += (s, e) =>
            {
                if (dgvPhieuNhap.CurrentRow == null) return;
                string maPN = dgvPhieuNhap.CurrentRow.Cells["MaPN"].Value?.ToString();
                if (!string.IsNullOrEmpty(maPN))
                    dgvChiTietPN.DataSource = _nhBus.GetChiTietPhieuNhap(maPN);
            };

            var lblDetail = new Label { Text = "Chi tiết các mặt hàng trong Phiếu nhập đang chọn:", Dock = DockStyle.Top, Height = 26, Font = UITheme.FontBold, ForeColor = UITheme.Primary };
            dgvChiTietPN = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvChiTietPN);

            pnlGrids.Controls.Add(dgvChiTietPN);
            pnlGrids.Controls.Add(lblDetail);
            pnlGrids.Controls.Add(dgvPhieuNhap);

            Controls.Add(pnlGrids);
            Controls.Add(grpNew);
            Controls.Add(pnlHeader);
        }

        private void LoadData()
        {
            txtMaPN.Text = _nhBus.SinhMaPhieuNhapMoi();
            cboNCC.DataSource = _dmBus.GetNhaCungCap();
            cboNCC.DisplayMember = "TenNCC";
            cboNCC.ValueMember = "MaNCC";

            cboSach.DataSource = _sachBus.TimKiem();
            cboSach.DisplayMember = "TenSach";
            cboSach.ValueMember = "MaSach";

            dgvPhieuNhap.DataSource = _nhBus.GetDanhSachPhieuNhap();
            if (dgvPhieuNhap.Rows.Count > 0)
            {
                string ma = dgvPhieuNhap.Rows[0].Cells["MaPN"].Value?.ToString();
                dgvChiTietPN.DataSource = _nhBus.GetChiTietPhieuNhap(ma);
            }
        }

        private void BtnNhapKho_Click(object sender, EventArgs e)
        {
            try
            {
                var pn = new PhieuNhap
                {
                    MaChungTu = txtMaPN.Text,
                    MaNV = SessionManager.CurrentUser?.MaNV ?? "NV04",
                    MaNCC = cboNCC.SelectedValue?.ToString(),
                    GhiChu = txtGhiChu.Text.Trim()
                };
                pn.ThemChiTiet(new ChiTietPhieuNhap
                {
                    MaSach = cboSach.SelectedValue?.ToString(),
                    TenSach = cboSach.Text,
                    SoLuong = (int)nudSoLuong.Value,
                    DonGia = nudGiaNhap.Value
                });
                _nhBus.LuuPhieuNhap(pn);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
