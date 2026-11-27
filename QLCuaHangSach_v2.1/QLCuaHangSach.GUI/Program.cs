using System.Windows.Forms;
using QLCuaHangSach.GUI;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        AppLog.Info("===== Khởi động ứng dụng =====");

        // Bộ xử lý lỗi toàn cục (Lab 15): lỗi không ai bắt sẽ được ghi log và báo thân thiện, ứng dụng không bị sập.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) => ErrorHandler.Show(null, e.Exception, "Lỗi không xác định");
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            AppLog.Error("Lỗi nghiêm trọng (UnhandledException)", e.ExceptionObject as Exception);

        while (true)
        {
            using (var login = new frmLogin())
            {
                if (login.ShowDialog() != DialogResult.OK) break;
            }

            bool dangXuat;
            using (var main = new frmMain())
            {
                Application.Run(main);
                dangXuat = main.DaDangXuat;
            }
            if (!dangXuat) break;   // chọn "Đăng xuất" thì quay lại màn hình đăng nhập, chọn "Thoát" thì đóng ứng dụng
        }

        AppLog.Info("===== Thoát ứng dụng =====");
    }
}
