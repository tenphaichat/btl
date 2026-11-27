using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;

namespace QLCuaHangSach.Reports
{
    /// <summary>
    /// Hiển thị một báo cáo Crystal Report: nạp file .rpt cùng tên báo cáo, gán dữ liệu (SetDataSource),
    /// truyền các tham số (SetParameterValue) và cho phép xuất PDF / Excel (Lab 13-14).
    /// </summary>
    public class frmReportViewer : Form
    {
        private const string TenBangThamSo = "ThamSo";

        private readonly string _tenBaoCao;
        private readonly string _tepDuLieu;
        private ReportDocument _report;
        private CrystalReportViewer _viewer;

        public frmReportViewer(string tenBaoCao, string tepDuLieu)
        {
            _tenBaoCao = tenBaoCao;
            _tepDuLieu = tepDuLieu;

            Text = "Báo cáo: " + tenBaoCao + " - Nhà sách Đông Á";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;

            var pnl = new Panel { Dock = DockStyle.Top, Height = 44 };
            var btnPdf = new Button { Text = "Xuất PDF", Location = new Point(10, 8), Size = new Size(110, 28) };
            btnPdf.Click += (s, e) => Xuat(ExportFormatType.PortableDocFormat, "PDF file|*.pdf", _tenBaoCao + ".pdf");
            var btnExcel = new Button { Text = "Xuất Excel", Location = new Point(130, 8), Size = new Size(110, 28) };
            btnExcel.Click += (s, e) => Xuat(ExportFormatType.ExcelWorkbook, "Excel Workbook|*.xls", _tenBaoCao + ".xls");
            pnl.Controls.Add(btnPdf);
            pnl.Controls.Add(btnExcel);

            _viewer = new CrystalReportViewer { Dock = DockStyle.Fill, ShowGroupTreeButton = false };

            Controls.Add(_viewer);
            Controls.Add(pnl);

            Load += (s, e) => NapBaoCao();
            FormClosed += (s, e) =>
            {
                if (_report != null) { _report.Close(); _report.Dispose(); }
            };
        }

        private void NapBaoCao()
        {
            try
            {
                var ds = new DataSet();
                ds.ReadXml(_tepDuLieu, XmlReadMode.ReadSchema);
                XoaTepTam();

                // Bảng dữ liệu chính = bảng không phải "ThamSo"; tên bảng khớp với tên bảng trong file .xsd/.rpt
                DataTable chinh = ds.Tables.Cast<DataTable>().FirstOrDefault(t => t.TableName != TenBangThamSo);
                if (chinh == null) throw new InvalidOperationException("Dữ liệu báo cáo trống.");

                string rpt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _tenBaoCao + ".rpt");
                if (!File.Exists(rpt))
                    throw new FileNotFoundException("Chưa có file thiết kế báo cáo: " + rpt +
                        "\nHãy thiết kế " + _tenBaoCao + ".rpt theo Docs/HuongDan_CrystalReport.md và đặt vào thư mục Rpt của project Reports.");

                _report = new ReportDocument();
                _report.Load(rpt);
                _report.SetDataSource(chinh);

                if (ds.Tables.Contains(TenBangThamSo))
                {
                    foreach (DataRow r in ds.Tables[TenBangThamSo].Rows)
                    {
                        try
                        {
                            _report.SetParameterValue(Convert.ToString(r["Ten"]), Convert.ToString(r["GiaTri"]));
                        }
                        catch (Exception)
                        {
                            // Báo cáo không khai báo tham số này (Parameter Field): bỏ qua.
                        }
                    }
                }

                _viewer.ReportSource = _report;
            }
            catch (Exception ex)
            {
                BaoLoi(ex);
            }
        }

        private void Xuat(ExportFormatType dinhDang, string boLoc, string tenMacDinh)
        {
            if (_report == null) return;
            using (var dlg = new SaveFileDialog { Filter = boLoc, FileName = tenMacDinh })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    _report.ExportToDisk(dinhDang, dlg.FileName);
                    MessageBox.Show(this, "Đã xuất báo cáo:\n" + dlg.FileName, "Xuất báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    BaoLoi(ex);
                }
            }
        }

        private void XoaTepTam()
        {
            try { File.Delete(_tepDuLieu); } catch { /* không quan trọng */ }
        }

        private void BaoLoi(Exception ex)
        {
            Program.GhiLog(ex);
            MessageBox.Show(this,
                "Không thể hiển thị/xuất báo cáo.\n\n" + ex.Message +
                "\n\nKiểm tra: (1) đã cài SAP Crystal Reports runtime đúng bản x86/x64 với PlatformTarget của project; " +
                "(2) file .rpt đã có trong thư mục chương trình.",
                "Lỗi báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
