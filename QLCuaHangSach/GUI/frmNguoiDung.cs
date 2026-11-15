using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.GUI
{
    /// <summary>Giao diện 5: Quản lý Khách hàng thân thiết & Điểm tích lũy.</summary>
    public class frmKhachHang : Form
    {
        private readonly KhachHangBUS _bus = new KhachHangBUS();
        public TextBox txtMaKH, txtHoTen, txtSDT, txtEmail, txtDiaChi, txtTimKiem;
        public RadioButton radNam, radNu;
        public NumericUpDown nudDiem;
        public DataGridView dgvKhachHang;
        public Label lblHangTV;
        private bool _isNew = false;

        public frmKhachHang()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Khách hàng Thân thiết & Tích điểm";
            Size = new Size(1050, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UITheme.BgLight;

            var pnlHeader = UITheme.CreateHeaderBanner(
                "QUẢN LÝ KHÁCH HÀNG THÂN THIẾT & HẠNG THÀNH VIÊN",
                "Tự động phân hạng Đồng / Bạc / Vàng / Kim cương theo điểm tích lũy mua sách");

            var grp = new GroupBox
            {
                Text = "Thông tin khách hàng",
                Font = UITheme.FontBold,
                Dock = DockStyle.Top,
                Height = 155,
                BackColor = Color.White
            };

            grp.Controls.Add(new Label { Text = "Mã KH:", Location = new Point(18, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtMaKH = new TextBox { Location = new Point(85, 27), Width = 100, ReadOnly = true, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Họ và tên:", Location = new Point(205, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtHoTen = new TextBox { Location = new Point(285, 27), Width = 240, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Giới tính:", Location = new Point(545, 30), AutoSize = true, Font = UITheme.FontRegular });
            radNam = new RadioButton { Text = "Nam", Location = new Point(620, 28), AutoSize = true, Checked = true, Font = UITheme.FontRegular };
            radNu = new RadioButton { Text = "Nữ", Location = new Point(680, 28), AutoSize = true, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Điểm TL:", Location = new Point(750, 30), AutoSize = true, Font = UITheme.FontRegular });
            nudDiem = new NumericUpDown { Location = new Point(820, 27), Width = 90, Maximum = 100000, Font = UITheme.FontRegular };

            lblHangTV = new Label { Text = "Hạng: Vàng", Location = new Point(920, 30), AutoSize = true, Font = UITheme.FontBold, ForeColor = UITheme.Warning };

            grp.Controls.Add(new Label { Text = "Điện thoại:", Location = new Point(18, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtSDT = new TextBox { Location = new Point(95, 65), Width = 160, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Email:", Location = new Point(275, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtEmail = new TextBox { Location = new Point(330, 65), Width = 230, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Địa chỉ:", Location = new Point(580, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtDiaChi = new TextBox { Location = new Point(645, 65), Width = 365, Font = UITheme.FontRegular };

            var btnMoi = UITheme.CreateButton("+ Thêm KH", UITheme.Accent, 120, 32);
            btnMoi.Location = new Point(95, 108);
            btnMoi.Click += (s, e) =>
            {
                _isNew = true;
                txtMaKH.Text = _bus.SinhMaKHMoi();
                txtHoTen.Clear();
                txtSDT.Clear();
                txtEmail.Clear();
                txtDiaChi.Clear();
                nudDiem.Value = 0;
            };

            var btnLuu = UITheme.CreateButton("Lưu KH", UITheme.Success, 120, 32);
            btnLuu.Location = new Point(225, 108);
            btnLuu.Click += BtnLuu_Click;

            var btnXoa = UITheme.CreateButton("Xóa KH", UITheme.Danger, 110, 32);
            btnXoa.Location = new Point(355, 108);
            btnXoa.Click += (s, e) => { try { _bus.XoaKhachHang(txtMaKH.Text); LoadGrid(); } catch (Exception ex) { MessageBox.Show(ex.Message); } };

            grp.Controls.Add( new Label { Text = "Tìm nhanh:", Location = new Point(545, 114), AutoSize = true, Font = UITheme.FontRegular });
            txtTimKiem = new TextBox { Location = new Point(625, 111), Width = 240, Font = UITheme.FontRegular };
            var btnTim = UITheme.CreateButton("Tìm", UITheme.Primary, 90, 30);
            btnTim.Location = new Point(875, 109);
            btnTim.Click += (s, e) => LoadGrid();

            grp.Controls.AddRange(new Control[]
            {
                txtMaKH, txtHoTen, radNam, radNu, nudDiem, lblHangTV,
                txtSDT, txtEmail, txtDiaChi, btnMoi, btnLuu, btnXoa, txtTimKiem, btnTim
            });

            dgvKhachHang = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvKhachHang);
            dgvKhachHang.SelectionChanged += DgvKhachHang_SelectionChanged;

            Controls.Add(dgvKhachHang);
            Controls.Add(grp);
            Controls.Add(pnlHeader);
        }

        private void LoadGrid()
        {
            dgvKhachHang.DataSource = _bus.GetAll(txtTimKiem?.Text?.Trim() ?? "");
        }

        private void DgvKhachHang_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKhachHang.CurrentRow == null) return;
            _isNew = false;
            var r = dgvKhachHang.CurrentRow;
            txtMaKH.Text = r.Cells["MaKH"].Value?.ToString();
            txtHoTen.Text = r.Cells["HoTen"].Value?.ToString();
            bool nam = r.Cells["GioiTinh"].Value?.ToString() == "Nam";
            radNam.Checked = nam;
            radNu.Checked = !nam;
            txtSDT.Text = r.Cells["SoDienThoai"].Value?.ToString();
            txtEmail.Text = r.Cells["Email"].Value?.ToString();
            txtDiaChi.Text = r.Cells["DiaChi"].Value?.ToString();
            int diem = Convert.ToInt32(r.Cells["DiemTichLuy"].Value ?? 0);
            nudDiem.Value = diem;
            var kh = new KhachHang { DiemTichLuy = diem };
            lblHangTV.Text = $"Hạng: {kh.HangThanhVien} (-{kh.TyLeGiamGia() * 100:0}%)";
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                var kh = new KhachHang
                {
                    MaKH = txtMaKH.Text.Trim(),
                    HoTen = txtHoTen.Text.Trim(),
                    GioiTinh = radNam.Checked,
                    SoDienThoai = txtSDT.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    DiaChi = txtDiaChi.Text.Trim(),
                    DiemTichLuy = (int)nudDiem.Value
                };
                _bus.LuuKhachHang(kh, _isNew);
                LoadGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    /// <summary>Giao diện 6: Quản lý Nhân viên & Phân quyền hệ thống.</summary>
    public class frmNhanVien : Form
    {
        private readonly AuthBUS _bus = new AuthBUS();
        public TextBox txtMaNV, txtHoTen, txtSDT, txtEmail, txtDiaChi, txtTaiKhoan, txtMatKhau;
        public ComboBox cboVaiTro;
        public CheckBox chkTrangThai;
        public DataGridView dgvNhanVien;
        private bool _isNew = false;

        public frmNhanVien()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            Text = "Quản lý Nhân viên & Phân quyền Tài khoản";
            Size = new Size(1040, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UITheme.BgLight;

            var pnlHeader = UITheme.CreateHeaderBanner(
                "QUẢN LÝ NHÂN VIÊN & PHÂN QUYỀN TRUY CẬP",
                "Phân quyền Quản lý / Nhân viên bán hàng / Thủ kho và bảo mật mật khẩu SHA-256");

            var grp = new GroupBox
            {
                Text = "Hồ sơ nhân viên & Tài khoản đăng nhập",
                Font = UITheme.FontBold,
                Dock = DockStyle.Top,
                Height = 155,
                BackColor = Color.White
            };

            grp.Controls.Add(new Label { Text = "Mã NV:", Location = new Point(18, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtMaNV = new TextBox { Location = new Point(85, 27), Width = 90, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Họ tên:", Location = new Point(195, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtHoTen = new TextBox { Location = new Point(260, 27), Width = 220, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Điện thoại:", Location = new Point(500, 30), AutoSize = true, Font = UITheme.FontRegular });
            txtSDT = new TextBox { Location = new Point(580, 27), Width = 150, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Vai trò:", Location = new Point(750, 30), AutoSize = true, Font = UITheme.FontRegular });
            cboVaiTro = new ComboBox { Location = new Point(815, 27), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };
            cboVaiTro.Items.AddRange(new object[] { "QuanLy", "BanHang", "ThuKho" });
            cboVaiTro.SelectedIndex = 1;

            grp.Controls.Add(new Label { Text = "Tài khoản:", Location = new Point(18, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtTaiKhoan = new TextBox { Location = new Point(95, 65), Width = 140, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Mật khẩu:", Location = new Point(255, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtMatKhau = new TextBox { Location = new Point(330, 65), Width = 150, PlaceholderText = "Mặc định 123456", Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Email:", Location = new Point(500, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtEmail = new TextBox { Location = new Point(555, 65), Width = 190, Font = UITheme.FontRegular };

            grp.Controls.Add(new Label { Text = "Địa chỉ:", Location = new Point(760, 68), AutoSize = true, Font = UITheme.FontRegular });
            txtDiaChi = new TextBox { Location = new Point(820, 65), Width = 175, Font = UITheme.FontRegular };

            chkTrangThai = new CheckBox { Text = "Đang làm việc", Location = new Point(95, 112), AutoSize = true, Checked = true, Font = UITheme.FontRegular };

            var btnMoi = UITheme.CreateButton("+ Thêm NV", UITheme.Accent, 120, 32);
            btnMoi.Location = new Point(240, 106);
            btnMoi.Click += (s, e) => { _isNew = true; txtMaNV.Text = "NV06"; txtHoTen.Clear(); txtTaiKhoan.Clear(); };

            var btnLuu = UITheme.CreateButton("Lưu Nhân viên", UITheme.Success, 135, 32);
            btnLuu.Location = new Point(370, 106);
            btnLuu.Click += (s, e) =>
            {
                try
                {
                    var nv = new NhanVien
                    {
                        MaNV = txtMaNV.Text.Trim(),
                        HoTen = txtHoTen.Text.Trim(),
                        SoDienThoai = txtSDT.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        DiaChi = txtDiaChi.Text.Trim(),
                        TenDangNhap = txtTaiKhoan.Text.Trim(),
                        VaiTro = cboVaiTro.SelectedItem?.ToString() ?? "BanHang",
                        TrangThai = chkTrangThai.Checked
                    };
                    _bus.SaveNhanVien(nv, txtMatKhau.Text, _isNew);
                    LoadGrid();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            };

            grp.Controls.AddRange(new Control[]
            {
                txtMaNV, txtHoTen, txtSDT, cboVaiTro,
                txtTaiKhoan, txtMatKhau, txtEmail, txtDiaChi,
                chkTrangThai, btnMoi, btnLuu
            });

            dgvNhanVien = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvNhanVien);
            dgvNhanVien.SelectionChanged += (s, e) =>
            {
                if (dgvNhanVien.CurrentRow == null) return;
                _isNew = false;
                var r = dgvNhanVien.CurrentRow;
                txtMaNV.Text = r.Cells["MaNV"].Value?.ToString();
                txtHoTen.Text = r.Cells["HoTen"].Value?.ToString();
                txtSDT.Text = r.Cells["SoDienThoai"].Value?.ToString();
                txtEmail.Text = r.Cells["Email"].Value?.ToString();
                txtDiaChi.Text = r.Cells["DiaChi"].Value?.ToString();
                txtTaiKhoan.Text = r.Cells["TenDangNhap"].Value?.ToString();
                cboVaiTro.SelectedItem = r.Cells["VaiTro"].Value?.ToString();
                chkTrangThai.Checked = Convert.ToBoolean(r.Cells["TrangThai"].Value ?? true);
            };

            Controls.Add(dgvNhanVien);
            Controls.Add(grp);
            Controls.Add(pnlHeader);
        }

        private void LoadGrid()
        {
            dgvNhanVien.DataSource = _bus.GetAllNhanVien();
        }
    }
}
