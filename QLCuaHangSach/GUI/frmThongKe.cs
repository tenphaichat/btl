using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI
{
    /// <summary>
    /// Giao diện 9: Trung tâm Báo cáo & Thống kê (Gọi các Stored Procedure theo yêu cầu đề bài:
    /// Tìm hóa đơn giá trị cao nhất, Tìm sách bán chạy nhất trong khoảng thời gian, Doanh thu tháng, Top KH).
    /// </summary>
    public class frmThongKe : Form
    {
        private readonly ThongKeBUS _bus = new ThongKeBUS();
        public ComboBox cboLoaiBaoCao;
        public DateTimePicker dtpTuNgay, dtpDenNgay;
        public NumericUpDown nudTop;
        public DataGridView dgvKetQua;
        public Label lblSummary;

        public frmThongKe()
        {
            InitializeComponent();
            ThucHienThongKe();
        }

        private void InitializeComponent()
        {
            Text = "Trung tâm Báo cáo, Thống kê & Truy vấn Nâng cao (Stored Procedures)";
            Size = new Size(1120, 660);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UITheme.BgLight;

            var pnlHeader = UITheme.CreateHeaderBanner(
                "TRUNG TÂM BÁO CÁO DOANH THU & TRUY VẤN STORED PROCEDURE",
                "Thực thi các Stored Procedure thống kê: Hóa đơn cao nhất, Sách bán chạy nhất theo thời gian, Doanh thu tháng");

            var pnlControl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            pnlControl.Controls.Add(new Label { Text = "Chọn báo cáo:", Location = new Point(14, 21), AutoSize = true, Font = UITheme.FontBold });
            cboLoaiBaoCao = new ComboBox
            {
                Location = new Point(115, 18),
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.FontRegular
            };
            cboLoaiBaoCao.Items.AddRange(new object[]
            {
                "1. Tìm sách bán chạy nhất trong khoảng thời gian (SP)",
                "2. Tìm hóa đơn có giá trị cao nhất (SP)",
                "3. Thống kê doanh thu & lợi nhuận 12 tháng năm 2026 (SP)",
                "4. Top khách hàng chi tiêu cao nhất (SP)",
                "5. Cảnh báo sách sắp hết hàng trong kho (SP)"
            });
            cboLoaiBaoCao.SelectedIndex = 0;
            cboLoaiBaoCao.SelectedIndexChanged += (s, e) => ThucHienThongKe();

            pnlControl.Controls.Add(new Label { Text = "Từ ngày:", Location = new Point(470, 21), AutoSize = true, Font = UITheme.FontRegular });
            dtpTuNgay = new DateTimePicker
            {
                Location = new Point(535, 18),
                Width = 115,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 1, 1),
                Font = UITheme.FontRegular
            };

            pnlControl.Controls.Add(new Label { Text = "Đến ngày:", Location = new Point(660, 21), AutoSize = true, Font = UITheme.FontRegular });
            dtpDenNgay = new DateTimePicker
            {
                Location = new Point(730, 18),
                Width = 115,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 12, 31),
                Font = UITheme.FontRegular
            };

            pnlControl.Controls.Add(new Label { Text = "Top:", Location = new Point(855, 21), AutoSize = true, Font = UITheme.FontRegular });
            nudTop = new NumericUpDown { Location = new Point(890, 18), Width = 55, Minimum = 1, Maximum = 100, Value = 10, Font = UITheme.FontRegular };

            var btnChay = UITheme.CreateButton("Truy vấn", UITheme.Primary, 85, 32);
            btnChay.Location = new Point(952, 16);
            btnChay.Click += (s, e) => ThucHienThongKe();

            var btnExcel = UITheme.CreateButton("Xuất Excel", UITheme.Success, 95, 32);
            btnExcel.Location = new Point(1042, 16);
            btnExcel.Click += (s, e) =>
            {
                using var dlg = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = "BaoCaoThongKe.xlsx" };
                if (dlg.ShowDialog() == DialogResult.OK && dgvKetQua.DataSource is DataTable dt)
                {
                    ExcelExportHelper.ExportDataTableToExcel(dt, dlg.FileName, cboLoaiBaoCao.Text);
                    MessageBox.Show("Đã xuất báo cáo Excel thành công!");
                }
            };

            pnlControl.Controls.AddRange(new Control[] { cboLoaiBaoCao, dtpTuNgay, dtpDenNgay, nudTop, btnChay, btnExcel });

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 38, BackColor = Color.White };
            lblSummary = new Label
            {
                Location = new Point(16, 9),
                AutoSize = true,
                Font = UITheme.FontBold,
                ForeColor = UITheme.Primary
            };
            pnlFooter.Controls.Add(lblSummary);

            dgvKetQua = new DataGridView { Dock = DockStyle.Fill };
            UITheme.StyleDataGridView(dgvKetQua);

            Controls.Add(dgvKetQua);
            Controls.Add(pnlFooter);
            Controls.Add(pnlControl);
            Controls.Add(pnlHeader);
        }

        public void ThucHienThongKe()
        {
            DataTable dt = cboLoaiBaoCao.SelectedIndex switch
            {
                0 => _bus.SachBanChay(dtpTuNgay.Value, dtpDenNgay.Value, (int)nudTop.Value),
                1 => _bus.TimHoaDonGiaTriCaoNhat(dtpTuNgay.Value, dtpDenNgay.Value),
                2 => _bus.DoanhThuTheoThang(2026),
                3 => _bus.TopKhachHang((int)nudTop.Value),
                _ => _bus.SachSapHetHang(25)
            };

            dgvKetQua.DataSource = dt;
            foreach (DataGridViewColumn col in dgvKetQua.Columns)
            {
                if (col.Name is "DoanhThu" or "LoiNhuan" or "TongTien" or "GiamGia" or "ThanhToan" or "TongChiTieu" or "GiaBan")
                    col.DefaultCellStyle.Format = "N0";
            }
            lblSummary.Text = $"Kết quả truy vấn [{cboLoaiBaoCao.Text}]: {dt.Rows.Count} bản ghi (Khoảng thời gian: {dtpTuNgay.Value:dd/MM/yyyy} - {dtpDenNgay.Value:dd/MM/yyyy})";
        }
    }
}
