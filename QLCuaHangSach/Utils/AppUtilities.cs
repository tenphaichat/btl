using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.Utils
{
    /// <summary>
    /// Quản lý phiên đăng nhập hiện tại của người dùng trong toàn ứng dụng.
    /// </summary>
    public static class SessionManager
    {
        public static NhanVien CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser != null && CurrentUser.LaQuanLy;

        public static void Login(NhanVien nv) => CurrentUser = nv;

        public static void Logout() => CurrentUser = null;
    }

    /// <summary>
    /// Tiện ích băm mật khẩu SHA-256 (tương thích với HASHBYTES('SHA2_256', ...) của SQL Server).
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>Class Method 1: Băm chuỗi mật khẩu sang chuỗi HEX SHA-256 viết hoa.</summary>
        public static string HashPasswordSHA256(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        /// <summary>Class Method 2: Kiểm tra mật khẩu nhập vào có khớp với mã băm hay không.</summary>
        public static bool VerifyPassword(string plainText, string expectedHash) =>
            string.Equals(HashPasswordSHA256(plainText), expectedHash, StringComparison.OrdinalIgnoreCase);
    }

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

    /// <summary>
    /// Tiện ích xuất bảng dữ liệu DataTable / DataGridView ra tệp Excel (.xlsx).
    /// </summary>
    public static class ExcelExportHelper
    {
        /// <summary>Class Method 8: Xuất DataTable ra tệp Excel có định dạng tiêu đề và bảng biểu.</summary>
        public static void ExportDataTableToExcel(System.Data.DataTable table, string filePath, string reportTitle, string subTitle = "")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("BaoCao");

            int colCount = Math.Max(1, table.Columns.Count);
            ws.Cell(1, 1).Value = "NHÀ SÁCH ĐÔNG Á - HỆ THỐNG QUẢN LÝ CỬA HÀNG SÁCH";
            ws.Range(1, 1, 1, colCount).Merge().Style.Font.SetBold().Font.SetFontSize(12);

            ws.Cell(2, 1).Value = reportTitle.ToUpperInvariant();
            ws.Range(2, 1, 2, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            if (!string.IsNullOrWhiteSpace(subTitle))
            {
                ws.Cell(3, 1).Value = subTitle;
                ws.Range(3, 1, 3, colCount).Merge().Style
                    .Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            int headerRow = 5;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var cell = ws.Cell(headerRow, c + 1);
                cell.Value = table.Columns[c].ColumnName;
                cell.Style.Font.SetBold();
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#1E3A8A"));
                cell.Style.Font.SetFontColor(XLColor.White);
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }

            for (int r = 0; r < table.Rows.Count; r++)
            {
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    var cell = ws.Cell(headerRow + 1 + r, c + 1);
                    object val = table.Rows[r][c];
                    if (val == null || val == DBNull.Value)
                        cell.Value = "";
                    else if (val is decimal dec)
                    {
                        cell.Value = dec;
                        cell.Style.NumberFormat.Format = "#,##0";
                    }
                    else if (val is int or long or double)
                        cell.Value = Convert.ToDouble(val);
                    else if (val is DateTime dt)
                    {
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                    }
                    else
                        cell.Value = val.ToString();

                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(filePath);
        }
    }
}
