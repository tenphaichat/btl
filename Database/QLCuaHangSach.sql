/* =====================================================================
   BÀI TẬP LỚN - HỌC PHẦN LẬP TRÌNH .NET (EAUT)
   Đề tài 20: Xây dựng phần mềm quản lý cửa hàng sách bằng công nghệ .NET
   Script tạo CSDL QLCuaHangSach: bảng, ràng buộc, trigger, stored procedure,
   dữ liệu mẫu. Chạy trên SQL Server / (localdb)\MSSQLLocalDB
   ===================================================================== */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE master;
GO
IF DB_ID(N'QLCuaHangSach') IS NOT NULL
BEGIN
    ALTER DATABASE QLCuaHangSach SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLCuaHangSach;
END
GO
CREATE DATABASE QLCuaHangSach;
GO
USE QLCuaHangSach;
GO

/* ============================ 1. BẢNG ============================ */
CREATE TABLE NhanVien (
    MaNV         VARCHAR(10)   NOT NULL PRIMARY KEY,
    HoTen        NVARCHAR(100) NOT NULL,
    GioiTinh     BIT           NOT NULL DEFAULT 1,          -- 1: Nam, 0: Nữ
    NgaySinh     DATE          NULL,
    SoDienThoai  VARCHAR(15)   NULL,
    Email        VARCHAR(100)  NULL,
    DiaChi       NVARCHAR(200) NULL,
    TenDangNhap  VARCHAR(50)   NOT NULL UNIQUE,
    MatKhauHash  VARCHAR(64)   NOT NULL,                    -- SHA-256 (HEX)
    VaiTro       NVARCHAR(20)  NOT NULL DEFAULT N'BanHang',
    TrangThai    BIT           NOT NULL DEFAULT 1,          -- 1: đang làm việc
    NgayTao      DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_NhanVien_VaiTro CHECK (VaiTro IN (N'QuanLy', N'BanHang', N'ThuKho'))
);

CREATE TABLE KhachHang (
    MaKH         VARCHAR(10)   NOT NULL PRIMARY KEY,
    HoTen        NVARCHAR(100) NOT NULL,
    GioiTinh     BIT           NOT NULL DEFAULT 1,
    NgaySinh     DATE          NULL,
    SoDienThoai  VARCHAR(15)   NOT NULL UNIQUE,
    Email        VARCHAR(100)  NULL,
    DiaChi       NVARCHAR(200) NULL,
    DiemTichLuy  INT           NOT NULL DEFAULT 0 CHECK (DiemTichLuy >= 0),
    NgayDangKy   DATE          NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);

CREATE TABLE TheLoai (
    MaTL   VARCHAR(10)   NOT NULL PRIMARY KEY,
    TenTL  NVARCHAR(100) NOT NULL UNIQUE,
    MoTa   NVARCHAR(255) NULL
);

CREATE TABLE TacGia (
    MaTG      VARCHAR(10)   NOT NULL PRIMARY KEY,
    TenTG     NVARCHAR(100) NOT NULL,
    QuocTich  NVARCHAR(50)  NULL
);

CREATE TABLE NhaXuatBan (
    MaNXB   VARCHAR(10)   NOT NULL PRIMARY KEY,
    TenNXB  NVARCHAR(150) NOT NULL UNIQUE,
    DiaChi  NVARCHAR(200) NULL
);

CREATE TABLE NhaCungCap (
    MaNCC        VARCHAR(10)   NOT NULL PRIMARY KEY,
    TenNCC       NVARCHAR(150) NOT NULL,
    DiaChi       NVARCHAR(200) NULL,
    SoDienThoai  VARCHAR(15)   NULL,
    Email        VARCHAR(100)  NULL
);

CREATE TABLE Sach (
    MaSach      VARCHAR(10)    NOT NULL PRIMARY KEY,
    TenSach     NVARCHAR(200)  NOT NULL,
    MaTL        VARCHAR(10)    NOT NULL REFERENCES TheLoai(MaTL),
    MaTG        VARCHAR(10)    NOT NULL REFERENCES TacGia(MaTG),
    MaNXB       VARCHAR(10)    NOT NULL REFERENCES NhaXuatBan(MaNXB),
    NamXuatBan  INT            NULL CHECK (NamXuatBan BETWEEN 1900 AND 2100),
    GiaNhap     DECIMAL(18,0)  NOT NULL DEFAULT 0 CHECK (GiaNhap >= 0),
    GiaBan      DECIMAL(18,0)  NOT NULL DEFAULT 0 CHECK (GiaBan >= 0),
    SoLuongTon  INT            NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    MoTa        NVARCHAR(500)  NULL,
    TrangThai   BIT            NOT NULL DEFAULT 1           -- 1: đang kinh doanh
);

CREATE TABLE HoaDon (
    MaHD       VARCHAR(15)    NOT NULL PRIMARY KEY,
    NgayLap    DATETIME       NOT NULL DEFAULT GETDATE(),
    MaNV       VARCHAR(10)    NOT NULL REFERENCES NhanVien(MaNV),
    MaKH       VARCHAR(10)    NULL REFERENCES KhachHang(MaKH),  -- NULL: khách lẻ
    TongTien   DECIMAL(18,0)  NOT NULL DEFAULT 0,
    GiamGia    DECIMAL(18,0)  NOT NULL DEFAULT 0,
    ThanhToan  DECIMAL(18,0)  NOT NULL DEFAULT 0,
    GhiChu     NVARCHAR(255)  NULL
);

CREATE TABLE ChiTietHoaDon (
    MaHD      VARCHAR(15)   NOT NULL REFERENCES HoaDon(MaHD) ON DELETE CASCADE,
    MaSach    VARCHAR(10)   NOT NULL REFERENCES Sach(MaSach),
    SoLuong   INT           NOT NULL CHECK (SoLuong > 0),
    DonGia    DECIMAL(18,0) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia) PERSISTED,
    CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaSach)
);

CREATE TABLE PhieuNhap (
    MaPN      VARCHAR(15)    NOT NULL PRIMARY KEY,
    NgayNhap  DATETIME       NOT NULL DEFAULT GETDATE(),
    MaNV      VARCHAR(10)    NOT NULL REFERENCES NhanVien(MaNV),
    MaNCC     VARCHAR(10)    NOT NULL REFERENCES NhaCungCap(MaNCC),
    TongTien  DECIMAL(18,0)  NOT NULL DEFAULT 0,
    GhiChu    NVARCHAR(255)  NULL
);

CREATE TABLE ChiTietPhieuNhap (
    MaPN        VARCHAR(15)   NOT NULL REFERENCES PhieuNhap(MaPN) ON DELETE CASCADE,
    MaSach      VARCHAR(10)   NOT NULL REFERENCES Sach(MaSach),
    SoLuong     INT           NOT NULL CHECK (SoLuong > 0),
    DonGiaNhap  DECIMAL(18,0) NOT NULL CHECK (DonGiaNhap >= 0),
    ThanhTien AS (SoLuong * DonGiaNhap) PERSISTED,
    CONSTRAINT PK_ChiTietPhieuNhap PRIMARY KEY (MaPN, MaSach)
);
GO

CREATE INDEX IX_HoaDon_NgayLap ON HoaDon(NgayLap);
CREATE INDEX IX_Sach_TenSach ON Sach(TenSach);
GO

/* ============================ 2. TRIGGER ============================ */
-- Bán hàng: trừ tồn kho, không cho phép tồn kho âm
CREATE TRIGGER trg_ChiTietHoaDon_Insert ON ChiTietHoaDon AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET s.SoLuongTon = s.SoLuongTon - i.SoLuong
    FROM Sach s JOIN inserted i ON s.MaSach = i.MaSach;
END
GO
-- Hủy hóa đơn / xóa dòng chi tiết: hoàn trả tồn kho
CREATE TRIGGER trg_ChiTietHoaDon_Delete ON ChiTietHoaDon AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET s.SoLuongTon = s.SoLuongTon + d.SoLuong
    FROM Sach s JOIN deleted d ON s.MaSach = d.MaSach;
END
GO
-- Nhập hàng: cộng tồn kho và cập nhật giá nhập mới nhất
CREATE TRIGGER trg_ChiTietPhieuNhap_Insert ON ChiTietPhieuNhap AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET s.SoLuongTon = s.SoLuongTon + i.SoLuong,
                 s.GiaNhap    = i.DonGiaNhap
    FROM Sach s JOIN inserted i ON s.MaSach = i.MaSach;
END
GO

/* ======================= 3. STORED PROCEDURE ======================= */
-- 3.1 Tìm hóa đơn có giá trị cao nhất (trong khoảng thời gian, NULL = toàn bộ)
CREATE PROCEDURE sp_TimHoaDonGiaTriCaoNhat
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 WITH TIES hd.MaHD, hd.NgayLap, nv.HoTen AS NhanVien,
           ISNULL(kh.HoTen, N'Khách lẻ') AS KhachHang,
           hd.TongTien, hd.GiamGia, hd.ThanhToan,
           (SELECT SUM(SoLuong) FROM ChiTietHoaDon WHERE MaHD = hd.MaHD) AS TongSoSach
    FROM HoaDon hd
    JOIN NhanVien nv ON nv.MaNV = hd.MaNV
    LEFT JOIN KhachHang kh ON kh.MaKH = hd.MaKH
    WHERE (@TuNgay IS NULL OR CAST(hd.NgayLap AS DATE) >= @TuNgay)
      AND (@DenNgay IS NULL OR CAST(hd.NgayLap AS DATE) <= @DenNgay)
    ORDER BY hd.ThanhToan DESC;
END
GO

-- 3.2 Tìm sách bán chạy nhất trong khoảng thời gian
CREATE PROCEDURE sp_SachBanChayTheoKhoangThoiGian
    @TuNgay DATE,
    @DenNgay DATE,
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) s.MaSach, s.TenSach, tl.TenTL AS TheLoai, tg.TenTG AS TacGia,
           SUM(ct.SoLuong) AS SoLuongBan,
           SUM(ct.ThanhTien) AS DoanhThu,
           COUNT(DISTINCT ct.MaHD) AS SoHoaDon
    FROM ChiTietHoaDon ct
    JOIN HoaDon hd ON hd.MaHD = ct.MaHD
    JOIN Sach s ON s.MaSach = ct.MaSach
    JOIN TheLoai tl ON tl.MaTL = s.MaTL
    JOIN TacGia tg ON tg.MaTG = s.MaTG
    WHERE CAST(hd.NgayLap AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY s.MaSach, s.TenSach, tl.TenTL, tg.TenTG
    ORDER BY SoLuongBan DESC, DoanhThu DESC;
END
GO

-- 3.3 Doanh thu, lợi nhuận theo từng tháng của một năm
CREATE PROCEDURE sp_DoanhThuTheoThang
    @Nam INT
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH Thang AS (SELECT 1 AS T UNION ALL SELECT T + 1 FROM Thang WHERE T < 12)
    SELECT t.T AS Thang,
           ISNULL(dt.SoHoaDon, 0)  AS SoHoaDon,
           ISNULL(dt.DoanhThu, 0)  AS DoanhThu,
           ISNULL(ln.LoiNhuan, 0)  AS LoiNhuan
    FROM Thang t
    LEFT JOIN (SELECT MONTH(NgayLap) AS T, COUNT(*) AS SoHoaDon, SUM(ThanhToan) AS DoanhThu
               FROM HoaDon WHERE YEAR(NgayLap) = @Nam GROUP BY MONTH(NgayLap)) dt ON dt.T = t.T
    LEFT JOIN (SELECT MONTH(hd.NgayLap) AS T, SUM(ct.SoLuong * (ct.DonGia - s.GiaNhap)) AS LoiNhuan
               FROM ChiTietHoaDon ct JOIN HoaDon hd ON hd.MaHD = ct.MaHD JOIN Sach s ON s.MaSach = ct.MaSach
               WHERE YEAR(hd.NgayLap) = @Nam GROUP BY MONTH(hd.NgayLap)) ln ON ln.T = t.T
    ORDER BY t.T;
END
GO

-- 3.4 Sách sắp hết hàng (tồn kho dưới ngưỡng)
CREATE PROCEDURE sp_SachSapHetHang
    @NguongTon INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.MaSach, s.TenSach, tl.TenTL AS TheLoai, nxb.TenNXB AS NhaXuatBan, s.SoLuongTon, s.GiaBan
    FROM Sach s
    JOIN TheLoai tl ON tl.MaTL = s.MaTL
    JOIN NhaXuatBan nxb ON nxb.MaNXB = s.MaNXB
    WHERE s.TrangThai = 1 AND s.SoLuongTon <= @NguongTon
    ORDER BY s.SoLuongTon ASC;
END
GO

-- 3.5 Top khách hàng mua nhiều nhất
CREATE PROCEDURE sp_TopKhachHang
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) kh.MaKH, kh.HoTen, kh.SoDienThoai, kh.DiemTichLuy,
           COUNT(hd.MaHD) AS SoLanMua, SUM(hd.ThanhToan) AS TongChiTieu
    FROM KhachHang kh
    JOIN HoaDon hd ON hd.MaKH = kh.MaKH
    GROUP BY kh.MaKH, kh.HoTen, kh.SoDienThoai, kh.DiemTichLuy
    ORDER BY TongChiTieu DESC;
END
GO

-- 3.6 Tìm kiếm sách đa tiêu chí
CREATE PROCEDURE sp_TimKiemSach
    @TuKhoa NVARCHAR(200) = N'',
    @MaTL VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.MaSach, s.TenSach, s.MaTL, tl.TenTL, s.MaTG, tg.TenTG, s.MaNXB, nxb.TenNXB,
           s.NamXuatBan, s.GiaNhap, s.GiaBan, s.SoLuongTon, s.MoTa, s.TrangThai
    FROM Sach s
    JOIN TheLoai tl ON tl.MaTL = s.MaTL
    JOIN TacGia tg ON tg.MaTG = s.MaTG
    JOIN NhaXuatBan nxb ON nxb.MaNXB = s.MaNXB
    WHERE (@TuKhoa = N'' OR s.TenSach LIKE N'%' + @TuKhoa + N'%' OR tg.TenTG LIKE N'%' + @TuKhoa + N'%'
           OR s.MaSach LIKE '%' + @TuKhoa + '%')
      AND (@MaTL IS NULL OR s.MaTL = @MaTL)
    ORDER BY s.MaSach;
END
GO

-- 3.7 Số liệu tổng quan cho màn hình chính (Dashboard)
CREATE PROCEDURE sp_ThongKeTongQuan
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
      (SELECT ISNULL(SUM(ThanhToan), 0) FROM HoaDon WHERE CAST(NgayLap AS DATE) = CAST(GETDATE() AS DATE)) AS DoanhThuHomNay,
      (SELECT ISNULL(SUM(ThanhToan), 0) FROM HoaDon WHERE YEAR(NgayLap) = YEAR(GETDATE()) AND MONTH(NgayLap) = MONTH(GETDATE())) AS DoanhThuThangNay,
      (SELECT COUNT(*) FROM HoaDon WHERE YEAR(NgayLap) = YEAR(GETDATE()) AND MONTH(NgayLap) = MONTH(GETDATE())) AS SoHoaDonThangNay,
      (SELECT COUNT(*) FROM Sach WHERE TrangThai = 1) AS SoDauSach,
      (SELECT ISNULL(SUM(SoLuongTon), 0) FROM Sach WHERE TrangThai = 1) AS TongTonKho,
      (SELECT COUNT(*) FROM KhachHang) AS SoKhachHang,
      (SELECT COUNT(*) FROM Sach WHERE TrangThai = 1 AND SoLuongTon <= 10) AS SoSachSapHet;
END
GO

/* ========================= 4. DỮ LIỆU MẪU ========================= */
-- Mật khẩu lưu dạng SHA-256: admin123 / 123456
INSERT INTO NhanVien (MaNV, HoTen, GioiTinh, NgaySinh, SoDienThoai, Email, DiaChi, TenDangNhap, MatKhauHash, VaiTro) VALUES
('NV01', N'Phan Văn Thắng',   1, '2006-03-26', '0912345601', 'thang.pv@bookstore.vn', N'Bắc Ninh',  'admin',   CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), N'QuanLy'),
('NV02', N'Nguyễn Thị Lan',   0, '2003-07-12', '0912345602', 'lan.nt@bookstore.vn',   N'Hà Nội',    'banhang', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2),   N'BanHang'),
('NV03', N'Trần Minh Đức',    1, '2002-11-03', '0912345603', 'duc.tm@bookstore.vn',   N'Bắc Giang', 'banhang2',CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2),   N'BanHang'),
('NV04', N'Lê Hoàng Nam',     1, '2001-05-20', '0912345604', 'nam.lh@bookstore.vn',   N'Bắc Ninh',  'thukho',  CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2),   N'ThuKho'),
('NV05', N'Phạm Thu Hà',      0, '2004-09-15', '0912345605', 'ha.pt@bookstore.vn',    N'Hải Dương', 'banhang3',CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2),   N'BanHang');

INSERT INTO TheLoai VALUES
('TL01', N'Văn học Việt Nam', N'Tiểu thuyết, truyện ngắn của tác giả Việt Nam'),
('TL02', N'Văn học nước ngoài', N'Tác phẩm dịch từ văn học thế giới'),
('TL03', N'Kỹ năng sống', N'Sách phát triển bản thân, kỹ năng mềm'),
('TL04', N'Kinh tế - Kinh doanh', N'Quản trị, tài chính, khởi nghiệp'),
('TL05', N'Công nghệ thông tin', N'Lập trình, khoa học máy tính'),
('TL06', N'Thiếu nhi', N'Truyện tranh, sách cho trẻ em'),
('TL07', N'Lịch sử - Địa lý', N'Sách lịch sử, văn hóa, địa lý'),
('TL08', N'Ngoại ngữ', N'Giáo trình, sách học ngoại ngữ');

INSERT INTO TacGia VALUES
('TG01', N'Nguyễn Nhật Ánh', N'Việt Nam'),
('TG02', N'Tô Hoài', N'Việt Nam'),
('TG03', N'Nam Cao', N'Việt Nam'),
('TG04', N'Paulo Coelho', N'Brazil'),
('TG05', N'Dale Carnegie', N'Mỹ'),
('TG06', N'Robert C. Martin', N'Mỹ'),
('TG07', N'Yuval Noah Harari', N'Israel'),
('TG08', N'Haruki Murakami', N'Nhật Bản'),
('TG09', N'Robert T. Kiyosaki', N'Mỹ'),
('TG10', N'Fujiko F. Fujio', N'Nhật Bản'),
('TG11', N'Antoine de Saint-Exupéry', N'Pháp'),
('TG12', N'Rosie Nguyễn', N'Việt Nam'),
('TG13', N'Andrew Hunt', N'Mỹ'),
('TG14', N'Trần Quốc Vượng', N'Việt Nam'),
('TG15', N'Nhiều tác giả', N'Việt Nam');

INSERT INTO NhaXuatBan VALUES
('NXB01', N'NXB Trẻ', N'161B Lý Chính Thắng, TP. Hồ Chí Minh'),
('NXB02', N'NXB Kim Đồng', N'55 Quang Trung, Hà Nội'),
('NXB03', N'NXB Văn học', N'18 Nguyễn Trường Tộ, Hà Nội'),
('NXB04', N'NXB Tổng hợp TP.HCM', N'62 Nguyễn Thị Minh Khai, TP. Hồ Chí Minh'),
('NXB05', N'NXB Lao động', N'175 Giảng Võ, Hà Nội'),
('NXB06', N'NXB Hội Nhà văn', N'65 Nguyễn Du, Hà Nội');

INSERT INTO NhaCungCap VALUES
('NCC01', N'Công ty Phát hành sách Fahasa', N'60-62 Lê Lợi, TP. Hồ Chí Minh', '02838225796', 'info@fahasa.com'),
('NCC02', N'Công ty Văn hóa Phương Nam', N'940 Ba Tháng Hai, TP. Hồ Chí Minh', '02838663447', 'contact@pnc.com.vn'),
('NCC03', N'Công ty CP Sách Alpha', N'176 Thái Hà, Hà Nội', '02437722176', 'info@alphabooks.vn'),
('NCC04', N'Công ty Văn hóa Nhã Nam', N'59 Đỗ Quang, Hà Nội', '02435146876', 'contact@nhanam.vn');

INSERT INTO Sach (MaSach, TenSach, MaTL, MaTG, MaNXB, NamXuatBan, GiaNhap, GiaBan, SoLuongTon, MoTa) VALUES
('S001', N'Mắt biếc',                              'TL01','TG01','NXB01',2019, 77000,110000, 60, N'Tiểu thuyết tình cảm học trò'),
('S002', N'Tôi thấy hoa vàng trên cỏ xanh',        'TL01','TG01','NXB01',2018, 87000,125000, 70, N'Tuổi thơ miền quê'),
('S003', N'Cho tôi xin một vé đi tuổi thơ',        'TL01','TG01','NXB01',2018, 56000, 80000, 55, N'Truyện dài'),
('S004', N'Dế Mèn phiêu lưu ký',                   'TL06','TG02','NXB02',2020, 35000, 50000, 80, N'Tác phẩm kinh điển thiếu nhi'),
('S005', N'Chí Phèo',                              'TL01','TG03','NXB03',2017, 28000, 40000, 45, N'Truyện ngắn hiện thực'),
('S006', N'Nhà giả kim',                           'TL02','TG04','NXB03',2020, 55000, 79000, 90, N'Tiểu thuyết triết lý'),
('S007', N'Đắc nhân tâm',                          'TL03','TG05','NXB04',2021, 60000, 86000,100, N'Nghệ thuật giao tiếp'),
('S008', N'Clean Code',                            'TL05','TG06','NXB05',2019,250000,350000, 30, N'Mã sạch và con đường trở thành lập trình viên giỏi'),
('S009', N'Sapiens: Lược sử loài người',           'TL07','TG07','NXB03',2021,175000,250000, 40, N'Lịch sử loài người'),
('S010', N'Rừng Na Uy',                            'TL02','TG08','NXB06',2018, 98000,140000, 35, N'Tiểu thuyết Nhật Bản'),
('S011', N'Cha giàu cha nghèo',                    'TL04','TG09','NXB01',2022, 70000,100000, 75, N'Giáo dục tài chính cá nhân'),
('S012', N'Doraemon - Tập 1',                      'TL06','TG10','NXB02',2023, 17000, 25000,120, N'Truyện tranh'),
('S013', N'Hoàng tử bé',                           'TL02','TG11','NXB06',2020, 45000, 65000, 65, N'Truyện triết lý'),
('S014', N'Tuổi trẻ đáng giá bao nhiêu',           'TL03','TG12','NXB06',2020, 56000, 80000, 85, N'Sách kỹ năng cho người trẻ'),
('S015', N'The Pragmatic Programmer',              'TL05','TG13','NXB05',2020,280000,399000, 25, N'Lập trình viên thực dụng'),
('S016', N'Tiếng Việt từ thời LM đến thời đổi mới','TL07','TG14','NXB03',2016, 95000,135000, 20, N'Nghiên cứu văn hóa'),
('S017', N'Vợ nhặt',                               'TL01','TG03','NXB03',2019, 25000, 36000, 50, N'Truyện ngắn'),
('S018', N'Kafka bên bờ biển',                     'TL02','TG08','NXB06',2021,120000,170000, 30, N'Tiểu thuyết'),
('S019', N'Nghĩ giàu làm giàu',                    'TL04','TG15','NXB05',2021, 77000,110000, 60, N'Kinh điển làm giàu'),
('S020', N'Homo Deus: Lược sử tương lai',          'TL07','TG07','NXB03',2022,190000,270000, 30, N'Tương lai nhân loại'),
('S021', N'Doraemon - Tập 2',                      'TL06','TG10','NXB02',2023, 17000, 25000,110, N'Truyện tranh'),
('S022', N'Hack não 1500 từ tiếng Anh',            'TL08','TG15','NXB05',2022,180000,259000, 40, N'Học từ vựng tiếng Anh'),
('S023', N'Ngữ pháp tiếng Anh cơ bản',             'TL08','TG15','NXB04',2021, 70000, 99000, 50, N'Giáo trình ngữ pháp'),
('S024', N'Lập trình C# căn bản',                  'TL05','TG15','NXB05',2023,105000,150000, 45, N'Giáo trình C# và .NET'),
('S025', N'Cà phê cùng Tony',                      'TL03','TG15','NXB01',2019, 63000, 90000, 70, N'Tản văn kỹ năng'),
('S026', N'Khởi nghiệp tinh gọn',                  'TL04','TG15','NXB05',2020,105000,150000, 35, N'Phương pháp Lean Startup'),
('S027', N'Kính vạn hoa - Tập 1',                  'TL06','TG01','NXB02',2021, 42000, 60000, 60, N'Truyện thiếu nhi'),
('S028', N'Ngồi khóc trên cây',                    'TL01','TG01','NXB01',2019, 77000,110000, 40, N'Truyện dài'),
('S029', N'Việt Nam sử lược',                      'TL07','TG15','NXB03',2020,126000,180000, 25, N'Lịch sử Việt Nam'),
('S030', N'Dế Mèn phiêu lưu ký (Bìa cứng)',        'TL06','TG02','NXB02',2022, 84000,120000, 30, N'Phiên bản đặc biệt');

INSERT INTO KhachHang (MaKH, HoTen, GioiTinh, NgaySinh, SoDienThoai, Email, DiaChi, NgayDangKy) VALUES
('KH001', N'Nguyễn Văn An',     1, '1998-02-10', '0987000001', 'an.nv@gmail.com',    N'Từ Sơn, Bắc Ninh',    '2026-01-05'),
('KH002', N'Trần Thị Bình',     0, '2001-06-21', '0987000002', 'binh.tt@gmail.com',  N'TP. Bắc Ninh',        '2026-01-12'),
('KH003', N'Lê Văn Cường',      1, '1995-11-30', '0987000003', 'cuong.lv@gmail.com', N'Yên Phong, Bắc Ninh', '2026-01-20'),
('KH004', N'Phạm Thị Dung',     0, '2000-04-14', '0987000004', NULL,                 N'Gia Lâm, Hà Nội',     '2026-02-02'),
('KH005', N'Hoàng Văn Em',      1, '2003-08-08', '0987000005', 'em.hv@gmail.com',    N'Tiên Du, Bắc Ninh',   '2026-02-15'),
('KH006', N'Vũ Thị Phương',     0, '1999-12-25', '0987000006', NULL,                 N'Quế Võ, Bắc Ninh',    '2026-03-01'),
('KH007', N'Đặng Văn Giang',    1, '1997-03-03', '0987000007', 'giang.dv@gmail.com', N'Đông Anh, Hà Nội',    '2026-03-18'),
('KH008', N'Bùi Thị Hạnh',      0, '2002-09-09', '0987000008', NULL,                 N'TP. Bắc Giang',       '2026-04-07'),
('KH009', N'Đỗ Văn Inh',        1, '1990-01-01', '0987000009', 'inh.dv@gmail.com',   N'Thuận Thành, Bắc Ninh','2026-04-22'),
('KH010', N'Ngô Thị Kim',       0, '2004-05-17', '0987000010', NULL,                 N'Long Biên, Hà Nội',   '2026-05-11'),
('KH011', N'Dương Văn Long',    1, '1996-07-07', '0987000011', 'long.dv@gmail.com',  N'Từ Sơn, Bắc Ninh',    '2026-06-03'),
('KH012', N'Lý Thị Mai',        0, '2005-10-10', '0987000012', NULL,                 N'Gia Bình, Bắc Ninh',  '2026-07-09'),
('KH013', N'Mai Văn Nam',       1, '1993-12-12', '0987000013', 'nam.mv@gmail.com',   N'Lương Tài, Bắc Ninh', '2026-08-14'),
('KH014', N'Tạ Thị Oanh',       0, '2001-02-28', '0987000014', NULL,                 N'TP. Hải Dương',       '2026-09-19'),
('KH015', N'Chu Văn Phúc',      1, '1999-06-06', '0987000015', 'phuc.cv@gmail.com',  N'Sóc Sơn, Hà Nội',     '2026-10-25');

-- Phiếu nhập mẫu (trigger tự cộng tồn kho)
INSERT INTO PhieuNhap (MaPN, NgayNhap, MaNV, MaNCC, GhiChu) VALUES
('PN0001', '2026-01-03 09:00', 'NV04', 'NCC01', N'Nhập hàng đầu năm'),
('PN0002', '2026-03-10 10:30', 'NV04', 'NCC03', N'Bổ sung sách kỹ năng'),
('PN0003', '2026-06-15 14:00', 'NV04', 'NCC04', N'Nhập sách văn học'),
('PN0004', '2026-09-01 08:30', 'NV04', 'NCC02', N'Chuẩn bị năm học mới'),
('PN0005', '2026-11-02 15:00', 'NV04', 'NCC01', N'Nhập sách CNTT');
INSERT INTO ChiTietPhieuNhap (MaPN, MaSach, SoLuong, DonGiaNhap) VALUES
('PN0001','S001',20, 77000),('PN0001','S002',20, 87000),('PN0001','S007',30, 60000),('PN0001','S012',40, 17000),
('PN0002','S014',25, 56000),('PN0002','S025',20, 63000),('PN0002','S011',20, 70000),
('PN0003','S006',30, 55000),('PN0003','S010',15, 98000),('PN0003','S013',20, 45000),
('PN0004','S022',20,180000),('PN0004','S023',20, 70000),('PN0004','S004',30, 35000),('PN0004','S021',40, 17000),
('PN0005','S008',10,250000),('PN0005','S015',10,280000),('PN0005','S024',20,105000);
UPDATE pn SET TongTien = x.Tong
FROM PhieuNhap pn JOIN (SELECT MaPN, SUM(ThanhTien) AS Tong FROM ChiTietPhieuNhap GROUP BY MaPN) x ON x.MaPN = pn.MaPN;
GO

-- Hóa đơn mẫu: sinh 72 hóa đơn trải đều từ 01/2026 đến 11/2026 + 3 hóa đơn trong ngày hiện tại
-- (trigger tự trừ tồn kho)
DECLARE @i INT = 1, @k INT, @soDong INT, @maHD VARCHAR(15), @ngay DATETIME, @maKH VARCHAR(10), @maNV VARCHAR(10), @maSach VARCHAR(10);
WHILE @i <= 75
BEGIN
    SET @maHD = 'HD' + RIGHT('0000' + CAST(@i AS VARCHAR(4)), 4);
    IF @i <= 72
        SET @ngay = DATEADD(MINUTE, (@i * 97) % 600, DATEADD(DAY, (@i - 1) * 44 / 10 + (@i % 3), CAST('2026-01-02 08:00' AS DATETIME)));
    ELSE
        SET @ngay = DATEADD(HOUR, 8 + (@i - 72) * 2, CAST(CAST(GETDATE() AS DATE) AS DATETIME));
    SET @maNV = CASE @i % 3 WHEN 0 THEN 'NV02' WHEN 1 THEN 'NV03' ELSE 'NV05' END;
    SET @maKH = CASE WHEN @i % 4 = 0 THEN NULL ELSE 'KH' + RIGHT('000' + CAST((@i * 7) % 15 + 1 AS VARCHAR(3)), 3) END;
    INSERT INTO HoaDon (MaHD, NgayLap, MaNV, MaKH) VALUES (@maHD, @ngay, @maNV, @maKH);

    SET @soDong = (@i % 4) + 1; SET @k = 1;
    WHILE @k <= @soDong
    BEGIN
        SET @maSach = 'S' + RIGHT('000' + CAST(((@i * 7 + @k * 11) % 30) + 1 AS VARCHAR(3)), 3);
        IF NOT EXISTS (SELECT 1 FROM ChiTietHoaDon WHERE MaHD = @maHD AND MaSach = @maSach)
            INSERT INTO ChiTietHoaDon (MaHD, MaSach, SoLuong, DonGia)
            SELECT @maHD, MaSach, ((@i + @k) % 3) + 1, GiaBan FROM Sach WHERE MaSach = @maSach;
        SET @k = @k + 1;
    END
    SET @i = @i + 1;
END

UPDATE hd SET TongTien = x.Tong,
       GiamGia = CASE WHEN hd.MaKH IS NOT NULL AND x.Tong >= 500000 THEN ROUND(x.Tong * 0.05, -3) ELSE 0 END
FROM HoaDon hd JOIN (SELECT MaHD, SUM(ThanhTien) AS Tong FROM ChiTietHoaDon GROUP BY MaHD) x ON x.MaHD = hd.MaHD;
UPDATE HoaDon SET ThanhToan = TongTien - GiamGia;

-- Điểm tích lũy: 1 điểm cho mỗi 10.000đ thanh toán
UPDATE kh SET DiemTichLuy = x.Diem
FROM KhachHang kh JOIN (SELECT MaKH, CAST(SUM(ThanhToan) / 10000 AS INT) AS Diem FROM HoaDon WHERE MaKH IS NOT NULL GROUP BY MaKH) x ON x.MaKH = kh.MaKH;

-- Một vài đầu sách tồn kho thấp để minh họa cảnh báo
UPDATE Sach SET SoLuongTon = 4 WHERE MaSach = 'S016';
UPDATE Sach SET SoLuongTon = 6 WHERE MaSach = 'S029';
UPDATE Sach SET SoLuongTon = 3 WHERE MaSach = 'S018';
GO

PRINT N'Khởi tạo CSDL QLCuaHangSach thành công.';
SELECT (SELECT COUNT(*) FROM Sach) AS Sach, (SELECT COUNT(*) FROM KhachHang) AS KhachHang,
       (SELECT COUNT(*) FROM HoaDon) AS HoaDon, (SELECT COUNT(*) FROM ChiTietHoaDon) AS ChiTietHD,
       (SELECT COUNT(*) FROM PhieuNhap) AS PhieuNhap, (SELECT SUM(ThanhToan) FROM HoaDon) AS TongDoanhThu;
GO
