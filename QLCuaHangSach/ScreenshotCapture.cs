using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using QLCuaHangSach.GUI;

namespace QLCuaHangSach
{
    public static class ScreenshotCapture
    {
        public static void RunAll(string outDir)
        {
            Directory.CreateDirectory(outDir);

            Capture(new frmLogin(), Path.Combine(outDir, "ui_01_login.png"));
            Capture(new frmMain(), Path.Combine(outDir, "ui_02_main_dashboard.png"));
            Capture(new frmSach(), Path.Combine(outDir, "ui_03_sach.png"));
            Capture(new frmDanhMuc(), Path.Combine(outDir, "ui_04_danhmuc.png"));
            Capture(new frmBanHang(), Path.Combine(outDir, "ui_05_banhang_pos.png"));
            Capture(new frmNhapHang(), Path.Combine(outDir, "ui_06_nhaphang.png"));
            Capture(new frmKhachHang(), Path.Combine(outDir, "ui_07_khachhang.png"));
            Capture(new frmNhanVien(), Path.Combine(outDir, "ui_08_nhanvien.png"));

            var frmTk1 = new frmThongKe();
            Capture(frmTk1, Path.Combine(outDir, "ui_09_thongke_banchay.png"));

            var frmTk2 = new frmThongKe();
            Capture(frmTk2, Path.Combine(outDir, "ui_10_thongke_hd_max.png"), f =>
            {
                ((frmThongKe)f).cboLoaiBaoCao.SelectedIndex = 1;
            });
        }

        private static void Capture(Form form, string path, Action<Form> beforeCapture = null)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(60, 60);
            form.Show();
            beforeCapture?.Invoke(form);
            Application.DoEvents();
            Thread.Sleep(220);
            Application.DoEvents();

            using var bmp = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
            bmp.Save(path, ImageFormat.Png);

            form.Close();
            form.Dispose();
        }
    }
}
