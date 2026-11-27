using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

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
        // Chọn sách thì đề xuất sẵn đơn giá nhập hiện tại của sách đó (người dùng vẫn sửa được)
        cboSach.SelectedIndexChanged += (s, e) =>
        {
            if (cboSach.SelectedItem is DataRowView drv && drv.Row.Table.Columns.Contains("GiaNhap"))
                nudGiaNhap.Value = Math.Min(nudGiaNhap.Maximum, Convert.ToDecimal(drv["GiaNhap"]));
        };

        grpNew.Controls.Add(new Label { Text = "SL nhập:", Location = new Point(16, 68), AutoSize = true, Font = UITheme.FontRegular });
        nudSoLuong = new NumericUpDown { Location = new Point(80, 65), Width = 85, Minimum = 1, Maximum = 5000, Value = 10, Font = UITheme.FontRegular };

        grpNew.Controls.Add(new Label { Text = "Đơn giá nhập:", Location = new Point(185, 68), AutoSize = true, Font = UITheme.FontRegular });
        nudGiaNhap = new NumericUpDown { Location = new Point(285, 65), Width = 140, Maximum = 50000000, Increment = 1000, ThousandsSeparator = true, Value = 0, Font = UITheme.FontRegular };

        grpNew.Controls.Add(new Label { Text = "Ghi chú:", Location = new Point(445, 68), AutoSize = true, Font = UITheme.FontRegular });
        txtGhiChu = new TextBox { Location = new Point(510, 65), Width = 325, Font = UITheme.FontRegular };

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
                MaNV = SessionManager.CurrentUser.MaNV,
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
            MessageBox.Show(this, $"Đã lưu phiếu nhập {pn.MaChungTu}. Tồn kho đã được cộng tự động.", "Nhập kho thành công",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtGhiChu.Clear();
            LoadData();
        }
        catch (Exception ex)
        {
            ErrorHandler.Show(this, ex);
        }
    }
}
