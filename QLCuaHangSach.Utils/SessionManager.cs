using QLCuaHangSach.DTO;

namespace QLCuaHangSach.Utils;

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
