# BÀI TẬP LỚN HỌC PHẦN LẬP TRÌNH .NET (IT3212)
## ĐỀ TÀI 20: XÂY DỰNG PHẦN MỀM QUẢN LÝ CỬA HÀNG SÁCH BẰNG CÔNG NGHỆ .NET

---

### 1. Thông tin chung
- **Trường:** Đại học Công nghệ Đông Á (EAUT) – Khoa Công nghệ Thông tin
- **Học phần:** Lập trình .NET – Lớp tín chỉ: `LTN.03 (IT3212)`
- **Giảng viên hướng dẫn:** ThS. Trần Nguyên Hoàng
- **Nhóm thực hiện:** Nhóm 2 – Đề tài 20:
  1. **Phan Văn Thắng** – MSSV: `24002545` (Nhóm Trưởng – Lớp `DCCNTT15.10`)
  2. **Võ Văn Cường** – MSSV: `24003135` (Thành viên – Lớp `DCCNTT15.10`)
  3. **Nguyễn Quang Dũng** – MSSV: `24003351` (Thành viên – Lớp `DCCNTT15.10`)
- **Nền tảng công nghệ:** C# Windows Forms (`.NET 9.0`) + Microsoft SQL Server (`(localdb)\MSSQLLocalDB` / SQL Server Express / Developer)
- **Kiến trúc phần mềm:** Kiến trúc phân tầng 3 lớp (**3-Tier Architecture**: `GUI` – `BUS` – `DAL` + `DTO` kế thừa đa cấp + `Utils` Class Methods)

---

### 2. Cấu trúc thư mục bàn giao
```text
btl_cuahangsach/
├── BaoCao_BTL_DeTai20_QLCuaHangSach.docx   # Bản báo cáo Word hoàn chỉnh theo đúng mẫu BTL 2026-2027
├── README.md                               # Hướng dẫn tổng quan bài tập lớn
├── Database/                               # Bản CSDL MDF & LDF và script SQL
├── docs/                                   # Sơ đồ thiết kế và hình ảnh giao diện
└── QLCuaHangSach_v2.1/                     # Toàn bộ mã nguồn hoàn chỉnh v2.1 (Kiến trúc 3-Tier đa dự án)
    ├── QLCuaHangSach.sln                   # Solution Visual Studio 2022 (.NET 9)
    ├── README.md                           # Chi tiết kiến trúc v2.1, Crystal Report, phân quyền, kiểm thử
    ├── Database/                           # Kịch bản SQL Server v2.1 (sp_Sach_PhanTrang, stored procedures)
    ├── Docs/                               # Kịch bản kiểm thử, ghi chú phiên bản, hướng dẫn Crystal Report
    ├── QLCuaHangSach.DTO/                  # Tầng Thực thể (cây kế thừa) + 6 interface
    ├── QLCuaHangSach.Utils/                # Tiện ích dùng chung (Logger, Session, Exception, Excel Export)
    ├── QLCuaHangSach.DAL/                  # Tầng Truy cập dữ liệu (ADO.NET, Stored Procedure, phân trang)
    ├── QLCuaHangSach.BUS/                  # Tầng Nghiệp vụ, phân quyền, chính sách chiết khấu, audit
    ├── QLCuaHangSach.GUI/                  # Tầng Giao diện người dùng WinForms (.NET 9)
    └── QLCuaHangSach.Reports/              # Dự án Báo cáo Crystal Report (.NET Framework 4.8)
```

---

### 3. Hướng dẫn khởi tạo Cơ sở dữ liệu và chạy chương trình

#### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
- Mở **SQL Server Management Studio (SSMS)** và chạy tệp `QLCuaHangSach_v2.1\Database\QLCuaHangSach.sql`, **HOẶC** chạy lệnh sau trong PowerShell / Command Prompt:
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -I -i "QLCuaHangSach_v2.1\Database\QLCuaHangSach.sql"
```
*(Nếu máy sử dụng SQL Server Express hoặc tên Server khác, chỉnh lại thuộc tính `connectionString` trong tệp `QLCuaHangSach_v2.1\QLCuaHangSach.GUI\App.config`).*

#### Bước 2: Biên dịch và khởi chạy ứng dụng
- Mở `QLCuaHangSach_v2.1\QLCuaHangSach.sln` bằng **Visual Studio 2022** và nhấn **F5 (Start)**, **HOẶC** chạy bằng `.NET CLI`:
```powershell
dotnet run --project QLCuaHangSach_v2.1\QLCuaHangSach.GUI\QLCuaHangSach.GUI.csproj
```

---

### 4. Tài khoản đăng nhập chạy thử (Demo Accounts)
Mật khẩu trong CSDL được băm tự động bằng thuật toán **SHA-256**:

| STT | Vai trò | Tên đăng nhập | Mật khẩu | Quyền hạn trong hệ thống |
|:---:|---------|:-------------:|:--------:|--------------------------|
| 1 | **Quản lý cửa hàng** | `admin` | `admin123` | Toàn quyền truy cập cả 9 màn hình chức năng |
| 2 | **Nhân viên bán hàng** | `banhang` | `123456` | Dashboard, Tra cứu sách, Bán hàng POS, Khách hàng, Thống kê |
| 3 | **Thủ kho** | `thukho` | `123456` | Dashboard, Quản lý Đầu sách, Danh mục nền, Nhập kho sách |
