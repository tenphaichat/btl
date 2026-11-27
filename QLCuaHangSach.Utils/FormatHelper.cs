using System.Globalization;

namespace QLCuaHangSach.Utils;

/// <summary>
/// Tiện ích định dạng tiền tệ, ngày tháng và sinh mã tự động.
/// </summary>
public static class FormatHelper
{
    private static readonly CultureInfo ViCulture = new CultureInfo("vi-VN");

    /// <summary>Class Method 6: Định dạng số tiền sang chuỗi VNĐ (ví dụ: 125.000 đ).</summary>
    public static string FormatCurrency(decimal amount) =>
        string.Format(ViCulture, "{0:N0} đ", amount);

    /// <summary>Class Method 7: Sinh mã tự động theo tiền tố và số thứ tự (ví dụ: HD0076, S031).</summary>
    public static string GenerateNextCode(string prefix, string lastCode, int digits = 3)
    {
        if (string.IsNullOrWhiteSpace(lastCode) || lastCode.Length <= prefix.Length)
            return prefix + 1.ToString().PadLeft(digits, '0');

        string numPart = lastCode.Substring(prefix.Length);
        if (int.TryParse(numPart, out int current))
            return prefix + (current + 1).ToString().PadLeft(digits, '0');

        return prefix + 1.ToString().PadLeft(digits, '0');
    }
}
