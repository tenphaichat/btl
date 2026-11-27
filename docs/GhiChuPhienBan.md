# Ghi chú phiên bản

## v2.1 (bổ sung công nghệ Lab 4-5, 8-9)
**Phải chạy lại script `Database\QLCuaHangSach.sql`** (có thêm `sp_Sach_PhanTrang`). Script xóa và tạo lại CSDL.

File thay đổi so với v2:
- Mới: `GUI\InputHelper.cs`, `GUI\TheoDoiThayDoi.cs`
- Sửa: `GUI\frmSach.cs`, `GUI\frmKhachHang.cs`, `GUI\frmNhanVien.cs`, `GUI\frmBanHang.cs`, `GUI\frmMain.cs`
- Sửa: `DAL\SachDAL.cs` (SearchPaged), `DAL\NhanVienDAL.cs` (GetLastCode), `BUS\SachBUS.cs`, `BUS\AuthBUS.cs`
- Sửa: `Database\QLCuaHangSach.sql`, `README.md`, `Docs\KichBanKiemThu.md`

Nội dung: ErrorProvider/KeyPress/FormClosing; tìm kiếm đa tiêu chí + sắp xếp + phân trang với SqlDataReader và tham số OUTPUT; sinh mã nhân viên tự động (thay mã `NV06` gõ cứng).

## v2
Tách 5 project, 6 interface, CRUD qua Stored Procedure, phân quyền thật, log + nhật ký + hiệu năng, Crystal Report (project .NET Framework 4.8 riêng). Xem `README.md`.
