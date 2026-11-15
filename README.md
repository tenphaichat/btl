# BÀI TẬP LỚN HỌC PHẦN LẬP TRÌNH .NET (IT3212)
## ĐỀ TÀI 20: XÂY DỰNG PHẦN MỀM QUẢN LÝ CỬA HÀNG SÁCH BẰNG CÔNG NGHỆ .NET

---

### 1. Thông tin chung
- **Trường:** Đại học Công nghệ Đông Á (EAUT) – Khoa Công nghệ Thông tin
- **Học phần:** Lập trình .NET – Lớp tín chỉ: `LTN.03 (IT3212)`
- **Giảng viên hướng dẫn:** ThS. Trần Nguyên Hoàng
- **Nhóm thực hiện:** Nhóm 20 – Nhóm trưởng: **Phan Văn Thắng** (`24002545` – Lớp `DCCNTT15.10`)
- **Nền tảng công nghệ:** C# Windows Forms (`.NET 9.0`) + Microsoft SQL Server (`(localdb)\MSSQLLocalDB` / SQL Server Express / Developer)
- **Kiến trúc phần mềm:** Kiến trúc phân tầng 3 lớp (**3-Tier Architecture**: `GUI` – `BUS` – `DAL` + `DTO` kế thừa đa cấp + `Utils` Class Methods)

---

### 2. Cấu trúc thư mục bàn giao
```text
btl_cuahangsach/
├── BaoCao_BTL_DeTai20_QLCuaHangSach.docx   # Bản báo cáo Word hoàn chỉnh theo đúng mẫu BTL 2026-2027
├── README.md                               # Hướng dẫn cài đặt, chạy chương trình và tài khoản demo
├── QLCuaHangSach.sln                       # Solution Visual Studio 2022 (.NET 9)
├── Database/
│   └── QLCuaHangSach.sql                   # Kịch bản tạo CSDL (11 bảng, 3 Trigger, 7 Stored Procedures + Dữ liệu mẫu)
└── QLCuaHangSach/
    ├── App.config                          # Cấu hình chuỗi kết nối SQL Server (Connection String)
    ├── Program.cs                          # Điểm khởi chạy ứng dụng
    ├── QLCuaHangSach.csproj                # Cấu hình dự án .NET 9 Windows Forms & thư viện ClosedXML
    ├── DTO/                                # Tầng Thực thể truyền tải dữ liệu (12 lớp, cây kế thừa 3 cấp)
    │   ├── EntityBase.cs                   # Lớp trừu tượng gốc EntityBase, ConNguoi, DanhMucBase
    │   ├── NguoiDTO.cs                     # NhanVien : ConNguoi, KhachHang : ConNguoi
    │   ├── DanhMucDTO.cs                   # TheLoai, TacGia, NhaXuatBan : DanhMucBase; NhaCungCap, Sach : EntityBase
    │   └── ChungTuDTO.cs                   # ChungTuBase<T>, HoaDon, PhieuNhap, ChiTietHoaDon, ChiTietPhieuNhap
    ├── DAL/                                # Tầng Truy cập dữ liệu (ADO.NET, SqlTransaction, Stored Procedures)
    │   ├── DatabaseConnection.cs           # Quản lý kết nối SQL Server tập trung
    │   ├── NguoiDAL.cs                     # Truy vấn bảng NhanVien, KhachHang
    │   ├── SachVaDanhMucDAL.cs             # Truy vấn bảng Sach, TheLoai, TacGia, NhaXuatBan, NhaCungCap
    │   └── ChungTuVaThongKeDAL.cs          # Giao dịch HoaDon, PhieuNhap (SqlTransaction) & gọi 7 Stored Procedures
    ├── BUS/                                # Tầng Xử lý nghiệp vụ (Validation Rules, tính chiết khấu, kiểm tra tồn kho)
    │   └── AppServicesBUS.cs               # AuthBUS, SachBUS, DanhMucBUS, KhachHangBUS, BanHangBUS, NhapHangBUS, ThongKeBUS
    ├── Utils/                              # Tầng Tiện ích dùng chung (8 Class Methods)
    │   └── AppUtilities.cs                 # SessionManager, SecurityHelper (SHA-256), ValidationHelper, FormatHelper, ExcelExportHelper
    └── GUI/                                # Tầng Giao diện người dùng (9 Forms hiện đại)
        ├── UITheme.cs                      # Bộ giao diện chuẩn Navy-Indigo & điều khiển tùy biến
        ├── frmLogin.cs                     # 1. Màn hình Đăng nhập hệ thống
        ├── frmMain.cs                      # 2. Màn hình chính & Bảng điều khiển tổng quan (Dashboard GDI+)
        ├── frmSachVaDanhMuc.cs             # 3. Quản lý Đầu sách (frmSach) & 4. Quản lý Danh mục nền (frmDanhMuc)
        ├── frmNghiepVu.cs                  # 5. Quầy thu ngân POS (frmBanHang) & 6. Nhập kho sách (frmNhapHang) + Xem trước hóa đơn
        ├── frmNguoiDung.cs                 # 7. Quản lý Khách hàng (frmKhachHang) & 8. Quản lý Nhân viên (frmNhanVien)
        └── frmThongKe.cs                   # 9. Báo cáo & Thống kê chuyên sâu (Sách bán chạy, Hóa đơn cao nhất, Doanh thu 12 tháng, Xuất Excel)
```

---

### 3. Hướng dẫn khởi tạo Cơ sở dữ liệu và chạy chương trình

#### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
- Mở **SQL Server Management Studio (SSMS)** và chạy tệp `Database\QLCuaHangSach.sql`, **HOẶC** chạy lệnh sau trong PowerShell / Command Prompt:
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -I -i "Database\QLCuaHangSach.sql"
```
*(Nếu máy sử dụng SQL Server Express hoặc tên Server khác, chỉnh lại thuộc tính `connectionString` trong tệp `QLCuaHangSach\App.config`).*

#### Bước 2: Biên dịch và khởi chạy ứng dụng
- Mở `QLCuaHangSach.sln` bằng **Visual Studio 2022** và nhấn **F5 (Start)**, **HOẶC** chạy bằng `.NET CLI`:
```powershell
dotnet run --project QLCuaHangSach\QLCuaHangSach.csproj
```

---

### 4. Tài khoản đăng nhập chạy thử (Demo Accounts)
Mật khẩu trong CSDL được băm tự động bằng thuật toán **SHA-256**:

| STT | Vai trò | Tên đăng nhập | Mật khẩu | Quyền hạn trong hệ thống |
|:---:|---------|:-------------:|:--------:|--------------------------|
| 1 | **Quản lý cửa hàng** | `admin` | `admin123` | Toàn quyền truy cập cả 9 màn hình chức năng |
| 2 | **Nhân viên bán hàng** | `banhang` | `123456` | Dashboard, Tra cứu sách, Bán hàng POS, Khách hàng, Thống kê |
| 3 | **Thủ kho** | `thukho` | `123456` | Dashboard, Quản lý Đầu sách, Danh mục nền, Nhập kho sách |
