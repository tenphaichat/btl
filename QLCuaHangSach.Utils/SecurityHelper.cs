using System.Security.Cryptography;
using System.Text;

namespace QLCuaHangSach.Utils;

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
