using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI
{
    /// <summary>Giao diện 3: Quản lý danh mục Sách (Tìm kiếm, lọc thể loại, thêm/sửa/ngừng KD, xuất Excel).</summary>
    public class frmSach : Form
    {
        private readonly SachBUS _sachBus = new SachBUS();
        private readonly DanhMucBUS _dmBus = new DanhMucBUS();

        public TextBox txtMaSach, txtTenSach, txtMoTa, txtTimKiem;
        public ComboBox cboTheLoai, cboTacGia, cboNXB, cboLocTheLoai;
        public NumericUpDown nudNamXB, nudGiaNhap, nudGiaBan, nudSoLuongTon;
        public DataGridView dgvSach;
        public Button btnThemMoi, btnLuu, btnXoa, btnTimKiem, btnXuatExcel;
        public Label lblStatus;
        private bool _isNew = false;

        public frmSach()
        {
            InitializeComponent();
            LoadLookups();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Danh mục Sách - Nhà sách Đông Á";
            Size = new Size(1120, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UITheme.BgLight;

            var pnlHeader = UITheme.CreateHeaderBanner(
                "QUẢN LÝ DANH MỤC ĐẦU SÁCH & TỒN KHO",
                "Tra cứu đa tiêu chí, cập nhật giá nhập/bán, quản lý số lượng tồn kho");

            var grpInput = new GroupBox
            {
                Text = "Thông tin chi tiết đầu sách",
                Font = UITheme.FontBold,
                Dock = DockStyle.Top,
                Height = 175,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            AddLabel(grpInput, "Mã sách:", 18, 30);
            txtMaSach = new TextBox { Location = new Point(95, 27), Width = 110, ReadOnly = true, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Tên sách:", 225, 30);
            txtTenSach = new TextBox { Location = new Point(300, 27), Width = 360, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Năm XB:", 680, 30);
            nudNamXB = new NumericUpDown { Location = new Point(750, 27), Width = 90, Minimum = 1900, Maximum = 2100, Value = 2024, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Tồn kho:", 860, 30);
            nudSoLuongTon = new NumericUpDown { Location = new Point(935, 27), Width = 100, Minimum = 0, Maximum = 100000, Value = 20, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Thể loại:", 18, 68);
            cboTheLoai = new ComboBox { Location = new Point(95, 65), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Tác giả:", 325, 68);
            cboTacGia = new ComboBox { Location = new Point(395, 65), Width = 265, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Nhà XB:", 680, 68);
            cboNXB = new ComboBox { Location = new Point(750, 65), Width = 285, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Giá nhập:", 18, 106);
            nudGiaNhap = new NumericUpDown { Location = new Point(95, 103), Width = 150, Maximum = 100000000, Increment = 1000, ThousandsSeparator = true, Value = 60000, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Giá bán:", 265, 106);
            nudGiaBan = new NumericUpDown { Location = new Point(335, 103), Width = 150, Maximum = 100000000, Increment = 1000, ThousandsSeparator = true, Value = 90000, Font = UITheme.FontRegular };

            AddLabel(grpInput, "Mô tả:", 505, 106);
            txtMoTa = new TextBox { Location = new Point(565, 103), Width = 470, Font = UITheme.FontRegular };

            btnThemMoi = UITheme.CreateButton("+ Tạo mã mới", UITheme.Accent, 130, 32);
            btnThemMoi.Location = new Point(95, 136);
            btnThemMoi.Click += (s, e) =>
            {
                _isNew = true;
                txtMaSach.Text = _sachBus.SinhMaSachMoi();
                txtTenSach.Clear();
                txtMoTa.Clear();
                nudGiaNhap.Value = 50000;
                nudGiaBan.Value = 80000;
                nudSoLuongTon.Value = 30;
                txtTenSach.Focus();
            };

            btnLuu = UITheme.CreateButton("Lưu thông tin", UITheme.Success, 135, 32);
            btnLuu.Location = new Point(235, 136);
            btnLuu.Click += BtnLuu_Click;

            btnXoa = UITheme.CreateButton("Ngừng KD", UITheme.Danger, 120, 32);
            btnXoa.Location = new Point(380, 136);
            btnXoa.Click += BtnXoa_Click;

            btnXuatExcel = UITheme.CreateButton("Xuất Excel", UITheme.Warning, 125, 32);
            btnXuatExcel.Location = new Point(510, 136);
            btnXuatExcel.Click += BtnXuatExcel_Click;

            grpInput.Controls.AddRange(new Control[]
            {
                txtMaSach, txtTenSach, nudNamXB, nudSoLuongTon,
                cboTheLoai, cboTacGia, cboNXB,
                nudGiaNhap, nudGiaBan, txtMoTa,
                btnThemMoi, btnLuu, btnXoa, btnXuatExcel
            });

            // Filter Bar
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(241, 245, 249) };
            AddLabel(pnlFilter, "Từ khóa:", 18, 13);
            txtTimKiem = new TextBox { Location = new Point(85, 10), Width = 280, Font = UITheme.FontRegular };
            AddLabel(pnlFilter, "Lọc thể loại:", 385, 13);
            cboLocTheLoai = new ComboBox { Location = new Point(475, 10), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };
            btnTimKiem = UITheme.CreateButton("Tìm kiếm", UITheme.Primary, 110, 30);
            btnTimKiem.Location = new Point(710, 8);
            btnTimKiem.Click += (s, e) => LoadGrid();
            lblStatus = new Label { Location = new Point(835, 13), AutoSize = true, Font = UITheme.FontBold, ForeColor = UITheme.Primary };
            pnlFilter.Controls.AddRange(new Control[] { txtTimKiem, cboLocTheLoai, btnTimKiem, lblStatus });

            dgvSach = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvSach);
            dgvSach.SelectionChanged += DgvSach_SelectionChanged;

            Controls.Add(dgvSach);
            Controls.Add(pnlFilter);
            Controls.Add(grpInput);
            Controls.Add(pnlHeader);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = UITheme.FontRegular });
        }

        private void LoadLookups()
        {
            cboTheLoai.DataSource = _dmBus.GetTheLoai();
            cboTheLoai.DisplayMember = "TenTL";
            cboTheLoai.ValueMember = "MaTL";

            cboTacGia.DataSource = _dmBus.GetTacGia();
            cboTacGia.DisplayMember = "TenTG";
            cboTacGia.ValueMember = "MaTG";

            cboNXB.DataSource = _dmBus.GetNhaXuatBan();
            cboNXB.DisplayMember = "TenNXB";
            cboNXB.ValueMember = "MaNXB";

            DataTable dtLoc = _dmBus.GetTheLoai();
            DataRow all = dtLoc.NewRow();
            all["MaTL"] = "";
            all["TenTL"] = "-- Tất cả thể loại --";
            dtLoc.Rows.InsertAt(all, 0);
            cboLocTheLoai.DataSource = dtLoc;
            cboLocTheLoai.DisplayMember = "TenTL";
            cboLocTheLoai.ValueMember = "MaTL";
        }

        public void LoadGrid()
        {
            string maTL = cboLocTheLoai.SelectedValue?.ToString();
            DataTable dt = _sachBus.TimKiem(txtTimKiem.Text.Trim(), maTL);
            dgvSach.DataSource = dt;
            if (dgvSach.Columns["MaTL"] != null) dgvSach.Columns["MaTL"].Visible = false;
            if (dgvSach.Columns["MaTG"] != null) dgvSach.Columns["MaTG"].Visible = false;
            if (dgvSach.Columns["MaNXB"] != null) dgvSach.Columns["MaNXB"].Visible = false;
            if (dgvSach.Columns["MaSach"] != null) dgvSach.Columns["MaSach"].HeaderText = "Mã sách";
            if (dgvSach.Columns["TenSach"] != null) dgvSach.Columns["TenSach"].HeaderText = "Tên đầu sách";
            if (dgvSach.Columns["TenTL"] != null) dgvSach.Columns["TenTL"].HeaderText = "Thể loại";
            if (dgvSach.Columns["TenTG"] != null) dgvSach.Columns["TenTG"].HeaderText = "Tác giả";
            if (dgvSach.Columns["TenNXB"] != null) dgvSach.Columns["TenNXB"].HeaderText = "Nhà xuất bản";
            if (dgvSach.Columns["NamXuatBan"] != null) dgvSach.Columns["NamXuatBan"].HeaderText = "Năm XB";
            if (dgvSach.Columns["GiaNhap"] != null) { dgvSach.Columns["GiaNhap"].HeaderText = "Giá nhập"; dgvSach.Columns["GiaNhap"].DefaultCellStyle.Format = "N0"; }
            if (dgvSach.Columns["GiaBan"] != null) { dgvSach.Columns["GiaBan"].HeaderText = "Giá bán"; dgvSach.Columns["GiaBan"].DefaultCellStyle.Format = "N0"; }
            if (dgvSach.Columns["SoLuongTon"] != null) dgvSach.Columns["SoLuongTon"].HeaderText = "Tồn kho";
            lblStatus.Text = $"Tổng số: {dt.Rows.Count} đầu sách";
        }

        private void DgvSach_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSach.CurrentRow == null) return;
            _isNew = false;
            DataGridViewRow r = dgvSach.CurrentRow;
            txtMaSach.Text = r.Cells["MaSach"].Value?.ToString();
            txtTenSach.Text = r.Cells["TenSach"].Value?.ToString();
            cboTheLoai.SelectedValue = r.Cells["MaTL"].Value?.ToString() ?? "";
            cboTacGia.SelectedValue = r.Cells["MaTG"].Value?.ToString() ?? "";
            cboNXB.SelectedValue = r.Cells["MaNXB"].Value?.ToString() ?? "";
            nudNamXB.Value = r.Cells["NamXuatBan"].Value != DBNull.Value ? Convert.ToDecimal(r.Cells["NamXuatBan"].Value) : 2024;
            nudGiaNhap.Value = Convert.ToDecimal(r.Cells["GiaNhap"].Value);
            nudGiaBan.Value = Convert.ToDecimal(r.Cells["GiaBan"].Value);
            nudSoLuongTon.Value = Convert.ToDecimal(r.Cells["SoLuongTon"].Value);
            txtMoTa.Text = r.Cells["MoTa"].Value?.ToString();
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                var s = new Sach
                {
                    MaSach = txtMaSach.Text.Trim(),
                    TenSach = txtTenSach.Text.Trim(),
                    MaTL = cboTheLoai.SelectedValue?.ToString(),
                    MaTG = cboTacGia.SelectedValue?.ToString(),
                    MaNXB = cboNXB.SelectedValue?.ToString(),
                    NamXuatBan = (int)nudNamXB.Value,
                    GiaNhap = nudGiaNhap.Value,
                    GiaBan = nudGiaBan.Value,
                    SoLuongTon = (int)nudSoLuongTon.Value,
                    MoTa = txtMoTa.Text.Trim()
                };
                _sachBus.LuuSach(s, _isNew);
                LoadGrid();
                lblStatus.Text = $"Đã lưu sách {s.MaSach} thành công.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSach.Text)) return;
            _sachBus.NgungKinhDoanh(txtMaSach.Text.Trim());
            LoadGrid();
        }

        private void BtnXuatExcel_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = "DanhMucSach.xlsx" };
            if (dlg.ShowDialog() == DialogResult.OK && dgvSach.DataSource is DataTable dt)
            {
                ExcelExportHelper.ExportDataTableToExcel(dt, dlg.FileName, "DANH MỤC ĐẦU SÁCH & TỒN KHO");
                MessageBox.Show("Đã xuất tệp Excel thành công!", "Thông báo");
            }
        }
    }

    /// <summary>Giao diện 4: Quản lý Danh mục Thể loại, Tác giả, Nhà xuất bản, Nhà cung cấp.</summary>
    public class frmDanhMuc : Form
    {
        private readonly DanhMucBUS _bus = new DanhMucBUS();
        private DataGridView _dgvTL, _dgvTG, _dgvNXB, _dgvNCC;
        private TextBox _txtMaTL, _txtTenTL, _txtMoTaTL;

        public frmDanhMuc()
        {
            InitializeComponent();
            LoadAllTabs();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Danh mục Dùng chung (Thể loại - Tác giả - NXB - Nhà cung cấp)";
            Size = new Size(980, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UITheme.BgLight;

            var pnlHeader = UITheme.CreateHeaderBanner(
                "QUẢN LÝ DANH MỤC THỂ LOẠI, TÁC GIẢ, NXB & NHÀ CUNG CẤP",
                "Chuẩn hóa dữ liệu danh mục phục vụ tra cứu và nhập/xuất sách");

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = UITheme.FontBold, Padding = new Point(14, 6) };

            var tabTL = new TabPage("1. Thể loại Sách");
            var pnlTLTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.White };
            pnlTLTop.Controls.Add(new Label { Text = "Mã TL:", Location = new Point(15, 16), AutoSize = true });
            _txtMaTL = new TextBox { Location = new Point(68, 13), Width = 80, Text = "TL01" };
            pnlTLTop.Controls.Add(new Label { Text = "Tên thể loại:", Location = new Point(165, 16), AutoSize = true });
            _txtTenTL = new TextBox { Location = new Point(255, 13), Width = 220, Text = "Văn học Việt Nam" };
            pnlTLTop.Controls.Add(new Label { Text = "Mô tả:", Location = new Point(490, 16), AutoSize = true });
            _txtMoTaTL = new TextBox { Location = new Point(545, 13), Width = 250, Text = "Tiểu thuyết, truyện ngắn" };
            var btnSaveTL = UITheme.CreateButton("Lưu Thể loại", UITheme.Success, 120, 30);
            btnSaveTL.Location = new Point(810, 11);
            btnSaveTL.Click += (s, e) => { _bus.LuuTheLoai(_txtMaTL.Text, _txtTenTL.Text, _txtMoTaTL.Text, false); LoadAllTabs(); };
            pnlTLTop.Controls.AddRange(new Control[] { _txtMaTL, _txtTenTL, _txtMoTaTL, btnSaveTL });

            _dgvTL = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(_dgvTL);
            tabTL.Controls.Add(_dgvTL);
            tabTL.Controls.Add(pnlTLTop);

            var tabTG = new TabPage("2. Tác giả");
            _dgvTG = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(_dgvTG);
            tabTG.Controls.Add(_dgvTG);

            var tabNXB = new TabPage("3. Nhà xuất bản");
            _dgvNXB = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(_dgvNXB);
            tabNXB.Controls.Add(_dgvNXB);

            var tabNCC = new TabPage("4. Nhà cung cấp");
            _dgvNCC = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(_dgvNCC);
            tabNCC.Controls.Add(_dgvNCC);

            tabs.TabPages.AddRange(new[] { tabTL, tabTG, tabNXB, tabNCC });

            Controls.Add(tabs);
            Controls.Add(pnlHeader);
        }

        private void LoadAllTabs()
        {
            _dgvTL.DataSource = _bus.GetTheLoai();
            _dgvTG.DataSource = _bus.GetTacGia();
            _dgvNXB.DataSource = _bus.GetNhaXuatBan();
            _dgvNCC.DataSource = _bus.GetNhaCungCap();
        }
    }
}
