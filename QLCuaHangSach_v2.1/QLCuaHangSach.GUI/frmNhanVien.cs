using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>Giao diện 6: Quản lý Nhân viên & Phân quyền hệ thống.</summary>
public class frmNhanVien : Form
{
    private readonly AuthBUS _bus = new AuthBUS();
    public TextBox txtMaNV, txtHoTen, txtSDT, txtEmail, txtDiaChi, txtTaiKhoan, txtMatKhau;
    public ComboBox cboVaiTro;
    public CheckBox chkTrangThai;
    public DataGridView dgvNhanVien;
    private bool _isNew = false;
    private readonly ErrorProvider _ep = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly TheoDoiThayDoi _theoDoi = new TheoDoiThayDoi();

    public frmNhanVien()
    {
        InitializeComponent();

        // Lab 5: KeyPress, theo dõi thay đổi + FormClosing
        txtSDT.MaxLength = 11;
        InputHelper.ChiChoNhapChuSo(txtSDT);
        _theoDoi.Theo(txtHoTen, txtSDT, txtEmail, txtDiaChi, txtTaiKhoan, txtMatKhau, cboVaiTro, chkTrangThai);
        InputHelper.XacNhanKhiDongNeuChuaLuu(this, () => _theoDoi.DaSua);

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
        txtMaNV = new TextBox { Location = new Point(85, 27), Width = 90, ReadOnly = true, Font = UITheme.FontRegular };

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
        btnMoi.Click += (s, e) =>
        {
            try
            {
                _isNew = true;
                _ep.Clear();
                _theoDoi.NapDuLieu(() =>
                {
                    txtMaNV.Text = _bus.SinhMaNhanVienMoi();
                    txtHoTen.Clear();
                    txtSDT.Clear();
                    txtEmail.Clear();
                    txtDiaChi.Clear();
                    txtTaiKhoan.Clear();
                    txtMatKhau.Clear();
                    chkTrangThai.Checked = true;
                });
                txtHoTen.Focus();
            }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
        };

        var btnLuu = UITheme.CreateButton("Lưu Nhân viên", UITheme.Success, 135, 32);
        btnLuu.Location = new Point(370, 106);
        btnLuu.Click += (s, e) =>
        {
            if (!ValidateInput()) return;
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
                _theoDoi.DanhDauDaLuu();
            }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
        };

        grp.Controls.AddRange(new Control[]
        {
            txtMaNV, txtHoTen, txtSDT, cboVaiTro,
            txtTaiKhoan, txtMatKhau, txtEmail, txtDiaChi,
            chkTrangThai, btnMoi, btnLuu
        });

        dgvNhanVien = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvNhanVien);
        dgvNhanVien.SelectionChanged += (s, e) => _theoDoi.NapDuLieu(HienThiDongDangChon);

        Controls.Add(dgvNhanVien);
        Controls.Add(grp);
        Controls.Add(pnlHeader);
    }

    private void LoadGrid()
    {
        dgvNhanVien.DataSource = _bus.GetAllNhanVien();
    }

    private void HienThiDongDangChon()
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
        _ep.Clear();
    }

    /// <summary>Kiểm tra dữ liệu bằng ErrorProvider (Lab 5).</summary>
    private bool ValidateInput()
    {
        _ep.Clear();
        bool hopLe = true;
        if (string.IsNullOrWhiteSpace(txtMaNV.Text))
        {
            _ep.SetError(txtMaNV, "Bấm \"+ Thêm NV\" để tạo mã mới, hoặc chọn một nhân viên trong danh sách.");
            hopLe = false;
        }
        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            _ep.SetError(txtHoTen, "Nhập họ tên nhân viên.");
            hopLe = false;
        }
        if (txtTaiKhoan.Text.Trim().Length < 4)
        {
            _ep.SetError(txtTaiKhoan, "Tên đăng nhập tối thiểu 4 ký tự.");
            hopLe = false;
        }
        if (_isNew && !string.IsNullOrEmpty(txtMatKhau.Text) && txtMatKhau.Text.Length < 6)
        {
            _ep.SetError(txtMatKhau, "Mật khẩu tối thiểu 6 ký tự (để trống sẽ dùng mật khẩu mặc định 123456).");
            hopLe = false;
        }
        if (!string.IsNullOrWhiteSpace(txtSDT.Text) && !ValidationHelper.IsValidPhone(txtSDT.Text.Trim()))
        {
            _ep.SetError(txtSDT, "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.");
            hopLe = false;
        }
        if (!ValidationHelper.IsValidEmail(txtEmail.Text.Trim()))
        {
            _ep.SetError(txtEmail, "Email không đúng định dạng.");
            hopLe = false;
        }
        return hopLe;
    }
}
