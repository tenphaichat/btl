using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>Giao diện 3: Quản lý danh mục Sách (Tìm kiếm, lọc thể loại, thêm/sửa/ngừng KD, xuất Excel).</summary>
public class frmSach : Form
{
    private readonly SachBUS _sachBus = new SachBUS();
    private readonly DanhMucBUS _dmBus = new DanhMucBUS();

    public TextBox txtMaSach, txtTenSach, txtMoTa, txtTimKiem;
    public ComboBox cboTheLoai, cboTacGia, cboNXB, cboLocTheLoai;
    public NumericUpDown nudNamXB, nudGiaNhap, nudGiaBan, nudSoLuongTon;
    public DataGridView dgvSach;
    public Button btnThemMoi, btnLuu, btnXoa, btnTimKiem, btnXuatExcel, btnTruoc, btnSau, btnDatLai;
    public NumericUpDown nudGiaTu, nudGiaDen;
    public ComboBox cboSapXep;
    public CheckBox chkGiamDan;
    public Label lblStatus, lblTrang;
    private bool _isNew = false;

    private const int SoDongMoiTrang = 10;   // Lab 9: mỗi trang 10 dòng, có nút Trước/Sau
    private int _trang = 1;
    private readonly ErrorProvider _ep = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly TheoDoiThayDoi _theoDoi = new TheoDoiThayDoi();

    public frmSach()
    {
        InitializeComponent();
        LoadLookups();

        // Lab 5: KeyPress (Enter để tìm), theo dõi thay đổi + FormClosing. Đăng ký sau LoadLookups để việc nạp combo không bị tính là "đã sửa".
        InputHelper.EnterDeThucHien(txtTimKiem, TimKiemTuTrangDau);
        _theoDoi.Theo(txtTenSach, txtMoTa, cboTheLoai, cboTacGia, cboNXB, nudNamXB, nudGiaNhap, nudGiaBan, nudSoLuongTon);
        InputHelper.XacNhanKhiDongNeuChuaLuu(this, () => _theoDoi.DaSua);

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
        nudNamXB = new NumericUpDown { Location = new Point(750, 27), Width = 90, Minimum = 1900, Maximum = 2100, Value = DateTime.Today.Year, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Tồn kho:", 860, 30);
        nudSoLuongTon = new NumericUpDown { Location = new Point(935, 27), Width = 100, Minimum = 0, Maximum = 100000, Value = 0, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Thể loại:", 18, 68);
        cboTheLoai = new ComboBox { Location = new Point(95, 65), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Tác giả:", 325, 68);
        cboTacGia = new ComboBox { Location = new Point(395, 65), Width = 265, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Nhà XB:", 680, 68);
        cboNXB = new ComboBox { Location = new Point(750, 65), Width = 285, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Giá nhập:", 18, 106);
        nudGiaNhap = new NumericUpDown { Location = new Point(95, 103), Width = 150, Maximum = 100000000, Increment = 1000, ThousandsSeparator = true, Value = 0, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Giá bán:", 265, 106);
        nudGiaBan = new NumericUpDown { Location = new Point(335, 103), Width = 150, Maximum = 100000000, Increment = 1000, ThousandsSeparator = true, Value = 0, Font = UITheme.FontRegular };

        AddLabel(grpInput, "Mô tả:", 505, 106);
        txtMoTa = new TextBox { Location = new Point(565, 103), Width = 470, Font = UITheme.FontRegular };

        btnThemMoi = UITheme.CreateButton("+ Tạo mã mới", UITheme.Accent, 130, 32);
        btnThemMoi.Location = new Point(95, 136);
        btnThemMoi.Click += (s, e) =>
        {
            try
            {
                _isNew = true;
                _ep.Clear();
                _theoDoi.NapDuLieu(() =>
                {
                    txtMaSach.Text = _sachBus.SinhMaSachMoi();
                    txtTenSach.Clear();
                    txtMoTa.Clear();
                    nudGiaNhap.Value = 0;     // giá phải do người dùng nhập; tồn kho tăng qua nghiệp vụ Nhập kho
                    nudGiaBan.Value = 0;
                    nudSoLuongTon.Value = 0;
                });
                txtTenSach.Focus();
            }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
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

        // Thanh tìm kiếm đa tiêu chí / sắp xếp (Lab 9)
        var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.FromArgb(241, 245, 249) };
        AddLabel(pnlFilter, "Từ khóa:", 18, 13);
        txtTimKiem = new TextBox { Location = new Point(85, 10), Width = 280, Font = UITheme.FontRegular };
        AddLabel(pnlFilter, "Lọc thể loại:", 385, 13);
        cboLocTheLoai = new ComboBox { Location = new Point(475, 10), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };
        btnTimKiem = UITheme.CreateButton("Tìm kiếm", UITheme.Primary, 110, 30);
        btnTimKiem.Location = new Point(710, 8);
        btnTimKiem.Click += (s, e) => TimKiemTuTrangDau();

        AddLabel(pnlFilter, "Giá bán từ:", 18, 55);
        nudGiaTu = new NumericUpDown { Location = new Point(95, 52), Width = 110, Maximum = 100000000, Increment = 10000, ThousandsSeparator = true, Font = UITheme.FontRegular };
        AddLabel(pnlFilter, "đến:", 215, 55);
        nudGiaDen = new NumericUpDown { Location = new Point(250, 52), Width = 110, Maximum = 100000000, Increment = 10000, ThousandsSeparator = true, Font = UITheme.FontRegular };
        pnlFilter.Controls.Add(new Label { Text = "(0 = không giới hạn)", Location = new Point(365, 56), AutoSize = true, Font = UITheme.FontRegular, ForeColor = Color.Gray });

        AddLabel(pnlFilter, "Sắp xếp theo:", 515, 55);
        cboSapXep = new ComboBox { Location = new Point(605, 52), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontRegular };
        cboSapXep.DisplayMember = "Ten";
        cboSapXep.ValueMember = "Ma";
        cboSapXep.DataSource = new[]
        {
            new { Ten = "Mã sách", Ma = "MaSach" },
            new { Ten = "Tên sách", Ma = "TenSach" },
            new { Ten = "Giá bán", Ma = "GiaBan" },
            new { Ten = "Tồn kho", Ma = "SoLuongTon" }
        };
        chkGiamDan = new CheckBox { Text = "Giảm dần", Location = new Point(745, 54), AutoSize = true, Font = UITheme.FontRegular };
        btnDatLai = UITheme.CreateButton("Đặt lại bộ lọc", UITheme.PrimaryDark, 130, 28);
        btnDatLai.Location = new Point(840, 50);
        btnDatLai.Click += (s, e) =>
        {
            txtTimKiem.Clear();
            if (cboLocTheLoai.Items.Count > 0) cboLocTheLoai.SelectedIndex = 0;
            nudGiaTu.Value = 0;
            nudGiaDen.Value = 0;
            cboSapXep.SelectedIndex = 0;
            chkGiamDan.Checked = false;
            TimKiemTuTrangDau();
        };
        pnlFilter.Controls.AddRange(new Control[] { txtTimKiem, cboLocTheLoai, btnTimKiem, nudGiaTu, nudGiaDen, cboSapXep, chkGiamDan, btnDatLai });

        // Thanh phân trang: Trước / Sau
        var pnlPhanTrang = new Panel { Dock = DockStyle.Bottom, Height = 42, BackColor = Color.White };
        btnTruoc = UITheme.CreateButton("< Trước", UITheme.PrimaryDark, 90, 28);
        btnTruoc.Location = new Point(12, 7);
        btnTruoc.Click += (s, e) => { if (_trang > 1) { _trang--; LoadGrid(); } };
        lblTrang = new Label { Location = new Point(112, 12), Width = 110, Font = UITheme.FontBold, ForeColor = UITheme.Primary };
        btnSau = UITheme.CreateButton("Sau >", UITheme.PrimaryDark, 90, 28);
        btnSau.Location = new Point(225, 7);
        btnSau.Click += (s, e) => { _trang++; LoadGrid(); };
        lblStatus = new Label { Location = new Point(335, 12), AutoSize = true, Font = UITheme.FontBold, ForeColor = UITheme.Primary };
        pnlPhanTrang.Controls.AddRange(new Control[] { btnTruoc, lblTrang, btnSau, lblStatus });

        dgvSach = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgvSach);
        dgvSach.SelectionChanged += DgvSach_SelectionChanged;

        Controls.Add(dgvSach);
        Controls.Add(pnlPhanTrang);
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

    private void TimKiemTuTrangDau()
    {
        _trang = 1;
        LoadGrid();
    }

    /// <summary>Lấy một trang kết quả theo bộ lọc hiện tại (từ khóa, thể loại, khoảng giá, cột sắp xếp).</summary>
    private DataTable LayDuLieu(int trang, int soDong, out int tongSo)
    {
        decimal? giaTu = nudGiaTu.Value > 0 ? nudGiaTu.Value : (decimal?)null;
        decimal? giaDen = nudGiaDen.Value > 0 ? nudGiaDen.Value : (decimal?)null;
        return _sachBus.TimKiemPhanTrang(txtTimKiem.Text.Trim(), cboLocTheLoai.SelectedValue?.ToString(),
            giaTu, giaDen, cboSapXep.SelectedValue?.ToString(), chkGiamDan.Checked, trang, soDong, out tongSo);
    }

    public void LoadGrid()
    {
        _ep.Clear();
        if (nudGiaDen.Value > 0 && nudGiaTu.Value > nudGiaDen.Value)
        {
            _ep.SetError(nudGiaDen, "Giá bán đến phải lớn hơn hoặc bằng giá bán từ.");
            return;
        }

        DataTable dt;
        int tongSo;
        int soTrang;
        try
        {
            dt = LayDuLieu(_trang, SoDongMoiTrang, out tongSo);
            soTrang = Math.Max(1, (tongSo + SoDongMoiTrang - 1) / SoDongMoiTrang);
            if (_trang > soTrang)   // ví dụ: sau khi lọc còn ít dòng hơn, lùi về trang cuối
            {
                _trang = soTrang;
                dt = LayDuLieu(_trang, SoDongMoiTrang, out tongSo);
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex);
            return;
        }

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
        lblTrang.Text = $"Trang {_trang} / {soTrang}";
        btnTruoc.Enabled = _trang > 1;
        btnSau.Enabled = _trang < soTrang;
        lblStatus.Text = $"Tổng số: {tongSo} đầu sách khớp điều kiện (mỗi trang {SoDongMoiTrang} dòng)";
    }

    // Chọn dòng trên lưới nạp dữ liệu lên ô nhập; việc nạp này không được tính là "người dùng đã sửa"
    private void DgvSach_SelectionChanged(object sender, EventArgs e) => _theoDoi.NapDuLieu(HienThiDongDangChon);

    private void HienThiDongDangChon()
    {
        if (dgvSach.CurrentRow == null) return;
        _isNew = false;
        _ep.Clear();
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

    /// <summary>Kiểm tra dữ liệu bằng ErrorProvider: biểu tượng lỗi hiện ngay cạnh ô sai (Lab 5).</summary>
    private bool ValidateInput()
    {
        _ep.Clear();
        bool hopLe = true;
        if (string.IsNullOrWhiteSpace(txtMaSach.Text))
        {
            _ep.SetError(txtMaSach, "Bấm \"+ Tạo mã mới\" hoặc chọn một sách trong danh sách.");
            hopLe = false;
        }
        if (string.IsNullOrWhiteSpace(txtTenSach.Text))
        {
            _ep.SetError(txtTenSach, "Nhập tên sách.");
            hopLe = false;
        }
        if (cboTheLoai.SelectedValue == null) { _ep.SetError(cboTheLoai, "Chọn thể loại."); hopLe = false; }
        if (cboTacGia.SelectedValue == null) { _ep.SetError(cboTacGia, "Chọn tác giả."); hopLe = false; }
        if (cboNXB.SelectedValue == null) { _ep.SetError(cboNXB, "Chọn nhà xuất bản."); hopLe = false; }
        if (nudGiaBan.Value <= 0)
        {
            _ep.SetError(nudGiaBan, "Nhập giá bán lớn hơn 0.");
            hopLe = false;
        }
        else if (nudGiaBan.Value < nudGiaNhap.Value)
        {
            _ep.SetError(nudGiaBan, "Giá bán phải lớn hơn hoặc bằng giá nhập.");
            hopLe = false;
        }
        return hopLe;
    }

    private void BtnLuu_Click(object sender, EventArgs e)
    {
        if (!ValidateInput()) return;
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
            _theoDoi.DanhDauDaLuu();
            lblStatus.Text = $"Đã lưu sách {s.MaSach} thành công.";
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex);
        }
    }

    private void BtnXoa_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtMaSach.Text)) return;
        if (MessageBox.Show(this, $"Ngừng kinh doanh sách {txtMaSach.Text}? Sách sẽ không bán được nữa nhưng vẫn giữ trong lịch sử hóa đơn.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            _sachBus.NgungKinhDoanh(txtMaSach.Text.Trim());
            LoadGrid();
        }
        catch (Exception ex) { ErrorHandler.Show(this, ex); }
    }

    private void BtnXuatExcel_Click(object sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = "DanhMucSach.xlsx" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            // Xuất toàn bộ kết quả khớp bộ lọc hiện tại (không chỉ trang đang xem)
            DataTable tatCa = LayDuLieu(1, 100000, out _);
            ExcelExportHelper.ExportDataTableToExcel(tatCa, dlg.FileName, "DANH MỤC ĐẦU SÁCH & TỒN KHO");
            MessageBox.Show("Đã xuất tệp Excel thành công!", "Thông báo");
        }
        catch (Exception ex) { ErrorHandler.Show(this, ex, "Không thể xuất Excel"); }
    }
}
