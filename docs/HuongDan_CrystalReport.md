# Hướng dẫn tạo 3 báo cáo Crystal Report (làm một lần)

Ứng dụng đã làm xong phần lập trình (lấy dữ liệu, gửi sang, hiển thị, xuất PDF/Excel). Phần còn lại là **thiết kế 3 file `.rpt`** trong Visual Studio, giống Lab 13-14.
File `.rpt` là định dạng nhị phân của SAP nên không thể tạo bằng mã; bạn thiết kế trong Report Designer.

## 0. Chuẩn bị
1. Cài **SAP Crystal Reports for Visual Studio** (bản tương thích VS 2022) và **runtime**.
2. Mở `QLCuaHangSach.sln`. Project `QLCuaHangSach.Reports` (.NET Framework 4.8) phải tải được.
3. `PlatformTarget` trong project Reports mặc định **x86**. Nếu bạn cài runtime 64-bit thì đổi thành **x64** (Properties → Build → Platform target).

## 1. Cách tạo một báo cáo (áp dụng cho cả 3)
1. Trong project **QLCuaHangSach.Reports**: chuột phải thư mục `Rpt` → *Add → New Item → Crystal Reports* → **Blank Report**, đặt tên **đúng tên báo cáo** (`HoaDon`, `DoanhThuThang`, `SachBanChay`).
2. Hộp thoại **Database Expert** → *Create New Connection* → **ADO.NET (XML)** → chọn file mẫu `Rpt\Schema\<TenBaoCao>_MauDuLieu.xml` (đã có kèm file `.xsd`).
3. Chọn bảng dữ liệu chính (tên ở mục dưới), đưa sang khung bên phải → OK.
4. Tạo **Parameter Fields** (Field Explorer → Parameter Fields → New): đặt đúng tên, kiểu **String**, **không** có dấu `?` ở tên.
5. Kéo các field vào vùng thiết kế theo bố cục gợi ý, định dạng số `#,##0`.
6. Lưu. Project tự chép `Rpt\*.rpt` cạnh file chạy khi build.

> Ứng dụng chính gửi dữ liệu thật có **cùng tên bảng và tên cột** với file mẫu. Tham số nào báo cáo không khai báo thì bị bỏ qua (không lỗi).

## 2. Ba báo cáo

### 2.1 `HoaDon.rpt` — Hóa đơn bán hàng
- Bảng dữ liệu: **`HoaDonChiTiet`** (mỗi dòng = một dòng chi tiết, kèm các cột đầu hóa đơn nên không cần liên kết bảng).
- Parameter Fields: `pTieuDe`, `pNguoiLap`, `pNgayLap`.
- Bố cục:
  - *Report Header:* tên cửa hàng, `?pTieuDe`.
  - *Group* theo `{HoaDonChiTiet.MaHD}` → *Group Header:* `MaHD`, `NgayLap`, `NhanVien`, `KhachHang`.
  - *Details:* `STT`, `TenSach`, `SoLuong`, `DonGia`, `ThanhTien`.
  - *Group Footer:* Tổng tiền hàng `Maximum({HoaDonChiTiet.TongTien}, {HoaDonChiTiet.MaHD})`, Giảm giá `Maximum({HoaDonChiTiet.GiamGia}, ...)`, **Khách cần trả** `Maximum({HoaDonChiTiet.ThanhToan}, ...)`.
  - *Page Footer:* `?pNguoiLap`, `?pNgayLap`, Page N of M.

### 2.2 `DoanhThuThang.rpt` — Doanh thu theo tháng
- Bảng: **`DoanhThuThang`** (cột `Thang`, `SoHoaDon`, `DoanhThu`, `LoiNhuan`).
- Parameter Fields: `pTieuDe`, `pNam`, `pNguoiLap`, `pNgayLap`.
- Bố cục: tiêu đề `?pTieuDe` + `Năm ?pNam`; *Details*: 4 cột; *Report Footer*: **Sum** `SoHoaDon`, `DoanhThu`, `LoiNhuan`; thêm **biểu đồ cột** (Insert → Chart: trục X `Thang`, giá trị `DoanhThu`) — yêu cầu Lab 14 bài 8.

### 2.3 `SachBanChay.rpt` — Sách bán chạy
- Bảng: **`SachBanChay`** (cột `MaSach`, `TenSach`, `TheLoai`, `TacGia`, `SoLuongBan`, `DoanhThu`, `SoHoaDon`).
- Parameter Fields: `pTieuDe`, `pTuNgay`, `pDenNgay`, `pNguoiLap`, `pNgayLap`.
- Bố cục: tiêu đề + `Từ ?pTuNgay đến ?pDenNgay`; *Details*: số thứ tự (`RecordNumber`), `TenSach`, `TheLoai`, `TacGia`, `SoLuongBan`, `DoanhThu`; *Report Footer*: Sum `SoLuongBan`, `DoanhThu`.

## 3. Chạy thử
1. Build solution (Debug). Project Reports xuất vào `QLCuaHangSach.GUI\bin\Debug\net9.0-windows\Reports\`.
2. Chạy ứng dụng chính → đăng nhập → *Thống kê → Crystal Report*, hoặc *Bán hàng* → thanh toán xong chọn *Yes* để in hóa đơn.
3. Cửa sổ báo cáo có nút **Xuất PDF** và **Xuất Excel**.

## 4. Lỗi thường gặp
| Hiện tượng | Nguyên nhân / cách xử lý |
|---|---|
| "Chưa tìm thấy chương trình báo cáo" | Chưa build project Reports (hoặc chọn Release trong khi chỉ build Debug) |
| "Chưa có file thiết kế báo cáo" | Chưa tạo/đặt đúng tên `.rpt` trong `Rpt\` rồi build lại |
| `BadImageFormatException` / không tải được Crystal | `PlatformTarget` (x86/x64) không khớp bản runtime đã cài |
| Báo cáo trống | Tên bảng/cột trong `.rpt` không khớp file mẫu; thiết kế lại từ file XML mẫu |
| Chi tiết lỗi | File `Logs\report_yyyyMMdd.log` cạnh `QLCuaHangSach.Reports.exe` |
