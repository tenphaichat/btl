using System.Text.RegularExpressions;

namespace QLCuaHangSach.Utils;

/// <summary>
/// Tiện ích kiểm tra dữ liệu đầu vào (Validation).
/// </summary>
public static class ValidationHelper
{
    private static readonly Regex PhoneRegex = new Regex(@"^0\d{9,10}$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    /// <summary>Class Method 3: Kiểm tra số điện thoại Việt Nam (10-11 chữ số, bắt đầu bằng 0).</summary>
    public static bool IsValidPhone(string phone) =>
        !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone.Trim());

    /// <summary>Class Method 4: Kiểm tra địa chỉ email hợp lệ.</summary>
    public static bool IsValidEmail(string email) =>
        string.IsNullOrWhiteSpace(email) || EmailRegex.IsMatch(email.Trim());

    /// <summary>Class Method 5: Kiểm tra khoảng thời gian hợp lệ (Từ ngày <= Đến ngày).</summary>
    public static bool IsValidDateRange(DateTime fromDate, DateTime toDate) =>
        fromDate.Date <= toDate.Date;
}
