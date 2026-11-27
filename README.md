# BÀI TẬP LỚN LẬP TRÌNH .NET (IT3212) - ĐỀ TÀI 20
## Phần mềm quản lý cửa hàng sách bằng công nghệ .NET

Trường Đại học Công nghệ Đông Á - Khoa Công nghệ Thông tin. Nhóm 2.

---

## 1. Công nghệ và kiến trúc

| Thành phần | Công nghệ |
|---|---|
| Ứng dụng chính | C# Windows Forms, **.NET 9** |
| Cơ sở dữ liệu | SQL Server (LocalDB / Express), ADO.NET (`System.Data.SqlClient`) |
| Báo cáo | **Crystal Report** chạy trong project riêng **.NET Framework 4.8** (`QLCuaHangSach.Reports`) + xuất Excel bằng ClosedXML |
| Kiến trúc | 3 lớp **GUI → BUS → DAL**, dùng chung **DTO** và **Utils**, mỗi lớp là một project riêng |

```
QLCuaHangSach.sln
├── QLCuaHangSach.DTO      Thực thể (cây kế thừa) + interface IValidatable, IPrintable, IExportable
├── QLCuaHangSach.Utils    Class Methods, BusinessException, DataAccessException, IAppLogger/FileLogger
├── QLCuaHangSach.DAL      ADO.NET, Stored Procedure, IRepository<T>, dịch lỗi SQL, AuditDAL
├── QLCuaHangSach.BUS      Nghiệp vụ, phân quyền, IDiscountPolicy, nhật ký + hiệu năng
├── QLCuaHangSach.GUI      WinForms (11 form), ErrorHandler, ReportLauncher        -> QLCuaHangSach.exe
├── QLCuaHangSach.Reports  Crystal Report viewer (.NET Framework 4.8)              -> Reports\QLCuaHangSach.Reports.exe
├── Database\QLCuaHangSach.sql   12 bảng, 3 Trigger, 47 Stored Procedure, dữ liệu mẫu
└── Docs\                  Hướng dẫn Crystal Report, kịch bản kiểm thử
```
Chiều tham chiếu: `GUI → BUS → DAL → DTO/Utils`. **GUI không tham chiếu trực tiếp DAL.**

### Hướng đối tượng
- Cây kế thừa: `EntityBase → ConNguoi → NhanVien / KhachHang`, `EntityBase → DanhMucBase → TheLoai / TacGia / NhaXuatBan`, `EntityBase → ChungTuBase<T> → HoaDon / PhieuNhap` (lớp trừu tượng, generic, virtual/override, đa hình).
- **6 interface**:
  | Interface | Nằm ở | Thực thi bởi / dùng ở |
  |---|---|---|
  | `IValidatable` | DTO | `EntityBase` (mọi thực thể); BUS gọi `IsValid(out msg)` |
  | `IPrintable` | DTO | `ChungTuBase<T>` (hóa đơn, phiếu nhập), `KhachHang` (thẻ thành viên); form xem trước hóa đơn |
  | `IExportable` | DTO | `ChungTuBase<T>`: `ToDataTable()` cho Excel/báo cáo |
  | `IRepository<T>` | DAL | `NhanVienDAL`, `KhachHangDAL`, `SachDAL`, `TheLoaiDAL`, `TacGiaDAL`, `NhaXuatBanDAL`, `NhaCungCapDAL`; `DanhMucBUS` dùng chung một đường xử lý |
  | `IDiscountPolicy` | BUS | `RankDiscountPolicy` (giảm theo hạng), `FlatRateDiscountPolicy` (đồng giá); `BanHangBUS` chỉ biết interface |
  | `IAppLogger` | Utils | `FileLogger` (ghi `Logs\yyyyMMdd.log`) |

---

## 2. Cài đặt và chạy

### Yêu cầu
- Visual Studio 2022 (workload *.NET desktop development*), .NET 9 SDK
- SQL Server LocalDB / Express + SSMS
- Để dùng báo cáo: **SAP Crystal Reports for Visual Studio** + runtime (bản x86 hoặc x64 khớp `PlatformTarget` của project Reports) — xem `Docs\HuongDan_CrystalReport.md`

### Các bước
1. Chạy `Database\QLCuaHangSach.sql` trong SSMS (script xóa và tạo lại CSDL `QLCuaHangSach`).
2. Mở `QLCuaHangSach.sln`, nếu cần sửa chuỗi kết nối trong `QLCuaHangSach.GUI\App.config`.
3. Build solution (Debug), đặt `QLCuaHangSach.GUI` làm Startup Project, chạy.
4. Thiết kế 3 file `.rpt` theo `Docs\HuongDan_CrystalReport.md` (chỉ cần làm một lần) để dùng chức năng báo cáo Crystal.

### Tài khoản dùng thử
| Tài khoản | Mật khẩu | Vai trò | Được dùng |
|---|---|---|---|
| `admin` | `admin123` | Quản lý | Tất cả, gồm Quản lý nhân viên và Quản trị hệ thống |
| `banhang` / `banhang2` / `banhang3` | `123456` | Nhân viên bán hàng | Trang chủ, Bán hàng (POS), Khách hàng, Thống kê |
| `thukho` | `123456` | Thủ kho | Trang chủ, Sách, Danh mục, Nhập kho |

Phân quyền được kiểm tra **hai lớp**: GUI ẩn mục không được phép; BUS (`PhanQuyenBUS.YeuCau`) chặn mọi thao tác ghi và ghi nhật ký `TU_CHOI_QUYEN`.
Sai mật khẩu 3 lần liên tiếp thì khóa đăng nhập 30 giây.

---

## 3. Các tính năng chính
- **Bán hàng (POS):** giỏ hàng bắt đầu rỗng; chặn thêm vượt tồn kho ngay khi thêm vào giỏ và lần nữa ở BUS; giảm giá theo hạng thành viên (Đồng 2%, Bạc 5%, Vàng 7%, Kim cương 10%, đơn từ 500.000 đ), nhãn giảm giá nói rõ lý do; xem trước hóa đơn; thanh toán trong một `SqlTransaction`, trigger tự trừ tồn kho và cộng điểm; lịch sử hóa đơn dạng master-detail.
- **Nhập kho:** lập phiếu nhập (transaction), trigger cộng tồn và cập nhật giá nhập gần nhất; master-detail phiếu nhập.
- **Danh mục:** đủ Thêm/Sửa/Xóa cho Thể loại, Tác giả, NXB, Nhà cung cấp (xóa bị chặn nếu đang được sử dụng).
- **CRUD qua Stored Procedure** cho 7 thực thể (`sp_<Bảng>_GetAll/GetById/Insert/Update/Delete`).
- **Nhập liệu (Lab 5):** `ErrorProvider` hiện lỗi ngay cạnh ô sai (Khách hàng, Nhân viên, Sách); `KeyPress` chặn ký tự không phải số ở ô điện thoại và Enter để tìm kiếm; `FormClosing` hỏi xác nhận khi còn thay đổi chưa lưu, khi giỏ hàng chưa thanh toán, và khi thoát ứng dụng.
- **Tìm kiếm nâng cao (Lab 8-9):** form Sách tìm theo từ khóa + thể loại + khoảng giá, sắp xếp theo cột hợp lệ (danh sách cố định, không nối tên cột vào SQL), **phân trang** 10 dòng bằng `OFFSET-FETCH`; DAL dùng `SqlDataReader` nạp vào `DataTable` và tham số **OUTPUT** `@TongSo` (`sp_Sach_PhanTrang`).
- **Quản trị (chỉ Quản lý):** nhật ký người dùng (bảng `AuditLog`), thống kê hiệu năng nghiệp vụ (thời gian xử lý trung bình/nhanh nhất/chậm nhất), xem file log lỗi.
- **Xử lý lỗi:** `BusinessException` (lỗi nghiệp vụ), `DataAccessException` (SQL Server đã dịch sang tiếng Việt), bộ xử lý lỗi toàn cục; lỗi chi tiết ghi vào `Logs\`, người dùng chỉ thấy thông báo thân thiện.
- **Báo cáo:** Crystal Report (Hóa đơn, Doanh thu theo tháng, Sách bán chạy) xuất PDF/Excel; xuất Excel bằng ClosedXML từ màn hình thống kê.

## 4. Lưu ý về Crystal Report và .NET 9
Crystal Reports không chạy trên .NET 9, nên báo cáo nằm trong project .NET Framework 4.8 riêng. Ứng dụng chính ghi dữ liệu báo cáo ra tệp XML tạm rồi mở `Reports\QLCuaHangSach.Reports.exe`; chương trình này nạp `.rpt`, gán dữ liệu và tham số, hiển thị và cho xuất PDF/Excel. Nếu chưa build project Reports hoặc chưa có `.rpt`, ứng dụng chính hiện thông báo hướng dẫn (không bị lỗi).

## 5. Kiểm thử
Xem `Docs\KichBanKiemThu.md` — bảng kịch bản và cột **Kết quả thực tế** để bạn tự chạy và điền.
