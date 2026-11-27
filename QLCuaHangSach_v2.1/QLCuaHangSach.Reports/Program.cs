using System;
using System.IO;
using System.Windows.Forms;

namespace QLCuaHangSach.Reports
{
    /// <summary>
    /// Chương trình báo cáo Crystal Report (.NET Framework 4.8).
    /// Được ứng dụng chính QLCuaHangSach (.NET 9) gọi tự động:  QLCuaHangSach.Reports.exe "TenBaoCao" "duongdan.xml"
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length < 2)
            {
                MessageBox.Show(
                    "Chương trình này được ứng dụng QLCuaHangSach gọi tự động khi bạn bấm \"In hóa đơn\" hoặc \"Crystal Report\".\n" +
                    "Không chạy trực tiếp.",
                    "Chương trình báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 1;
            }

            try
            {
                Application.Run(new frmReportViewer(args[0], args[1]));
                return 0;
            }
            catch (Exception ex)
            {
                GhiLog(ex);
                MessageBox.Show("Chương trình báo cáo gặp lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 2;
            }
        }

        /// <summary>Ghi lỗi vào Logs\report_yyyyMMdd.log cạnh file chạy; không bao giờ làm chương trình lỗi thêm.</summary>
        internal static void GhiLog(Exception ex)
        {
            try
            {
                string thuMuc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(thuMuc);
                string tep = Path.Combine(thuMuc, "report_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                File.AppendAllText(tep, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | ERROR | " + ex + Environment.NewLine);
            }
            catch
            {
                // bỏ qua
            }
        }
    }
}
