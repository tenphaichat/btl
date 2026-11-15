using System.IO;
using System.Windows.Forms;
using QLCuaHangSach.GUI;

namespace QLCuaHangSach
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            if (args != null && args.Length > 0 && args[0] == "--capture")
            {
                string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
                string imgDir = Path.Combine(root, "docs", "images");
                ScreenshotCapture.RunAll(imgDir);
                return;
            }

            using var login = new frmLogin();
            if (login.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new frmMain());
            }
        }
    }
}
