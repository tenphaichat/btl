using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

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
    private readonly ErrorProvider _ep = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly TheoDoiThayDoi _theoDoi = new TheoDoiThayDoi();

    public frmKhachHang()
    {
        InitializeComponent();

        // Lab 5: KeyPress, theo dõi thay đổi + FormClosing
        txtSDT.MaxLength = 11;
        InputHelper.ChiChoNhapChuSo(txtSDT);
        InputHelper.EnterDeThucHien(txtTimKiem, LoadGrid);
        _theoDoi.Theo(txtHoTen, txtSDT, txtEmail, txtDiaChi, radNam, nudDiem);
        InputHelper.XacNhanKhiDongNeuChuaLuu(this, () => _theoDoi.DaSua);

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
            try
            {
                _isNew = true;
                _ep.Clear();
                _theoDoi.NapDuLieu(() =>
                {
                    txtMaKH.Text = _bus.SinhMaKHMoi();
                    txtHoTen.Clear();
                    txtSDT.Clear();
                    txtEmail.Clear();
                    txtDiaChi.Clear();
                    nudDiem.Value = 0;
                });
                txtHoTen.Focus();
            }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
        };

        var btnLuu = UITheme.CreateButton("Lưu KH", UITheme.Success, 120, 32);
        btnLuu.Location = new Point(225, 108);
        btnLuu.Click += BtnLuu_Click;

        var btnXoa = UITheme.CreateButton("Xóa KH", UITheme.Danger, 110, 32);
        btnXoa.Location = new Point(355, 108);
        btnXoa.Click += (s, e) => { try { _bus.XoaKhachHang(txtMaKH.Text); LoadGrid(); } catch (Exception ex) { ErrorHandler.Show(this, ex); } };

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

    // Chọn dòng trên lưới nạp dữ liệu lên ô nhập; việc nạp này không được tính là "người dùng đã sửa"
    private void DgvKhachHang_SelectionChanged(object sender, EventArgs e) => _theoDoi.NapDuLieu(HienThiDongDangChon);

    private void HienThiDongDangChon()
    {
        if (dgvKhachHang.CurrentRow == null) return;
        _isNew = false;
        _ep.Clear();
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
        lblHangTV.Text = $"Hạng: {kh.HangThanhVien} ({kh.TyLeGiamGia():P0})";
    }

    /// <summary>Kiểm tra dữ liệu bằng ErrorProvider: hiện biểu tượng lỗi ngay cạnh ô nhập sai (Lab 5).</summary>
    private bool ValidateInput()
    {
        _ep.Clear();
        bool hopLe = true;
        if (string.IsNullOrWhiteSpace(txtMaKH.Text))
        {
            _ep.SetError(txtMaKH, "Bấm \"+ Thêm KH\" để tạo mã mới, hoặc chọn một khách hàng trong danh sách.");
            hopLe = false;
        }
        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            _ep.SetError(txtHoTen, "Nhập họ và tên khách hàng.");
            hopLe = false;
        }
        if (!ValidationHelper.IsValidPhone(txtSDT.Text.Trim()))
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

    private void BtnLuu_Click(object sender, EventArgs e)
    {
        if (!ValidateInput()) return;
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
            _theoDoi.DanhDauDaLuu();
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex);
        }
    }
}
