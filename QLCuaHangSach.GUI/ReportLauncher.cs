using System.Data;
using System.Diagnostics;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>
/// Cầu nối giữa ứng dụng chính (.NET 9) và chương trình báo cáo Crystal Report (.NET Framework 4.8).
/// Crystal Reports không chạy được trên .NET 9 nên báo cáo được mở trong một tiến trình riêng:
/// ứng dụng chính ghi dữ liệu báo cáo ra tệp XML tạm rồi khởi chạy QLCuaHangSach.Reports.exe với tên báo cáo và đường dẫn tệp.
/// </summary>
public static class ReportLauncher
{
    private const string TenExe = "QLCuaHangSach.Reports.exe";

    /// <param name="tenBaoCao">Tên báo cáo = tên file .rpt (HoaDon, DoanhThuThang, SachBanChay).</param>
    public static void Mo(string tenBaoCao, DataSet duLieu)
    {
        string exe = TimChuongTrinhBaoCao();
        if (exe == null)
            throw new BusinessException(
                "Chưa tìm thấy chương trình báo cáo (" + TenExe + ").\n" +
                "Hãy build project QLCuaHangSach.Reports và cài SAP Crystal Reports runtime (xem Docs/HuongDan_CrystalReport.md).");

        string tep = Path.Combine(Path.GetTempPath(), $"QLCuaHangSach_{tenBaoCao}_{Guid.NewGuid():N}.xml");
        try
        {
            duLieu.WriteXml(tep, XmlWriteMode.WriteSchema);
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"\"{tenBaoCao}\" \"{tep}\"",
                WorkingDirectory = Path.GetDirectoryName(exe),
                UseShellExecute = false
            });
            AppLog.Info($"Mở báo cáo Crystal '{tenBaoCao}'");
        }
        catch (Exception ex)
        {
            AppLog.Error("Không mở được chương trình báo cáo", ex);
            throw new BusinessException("Không mở được chương trình báo cáo: " + ex.Message, ex);
        }
    }

    private static string TimChuongTrinhBaoCao()
    {
        string goc = AppContext.BaseDirectory;
        foreach (string duongDan in new[] { Path.Combine(goc, "Reports", TenExe), Path.Combine(goc, TenExe) })
            if (File.Exists(duongDan)) return duongDan;
        return null;
    }
}
