# Kịch bản kiểm thử (tự chạy và điền kết quả thật)

Điền cột **Kết quả thực tế** và **Đạt/Không** sau khi chạy. Không ghi "Đạt" cho kịch bản bạn chưa chạy.
Môi trường kiểm thử: ........................ (Windows, SQL Server, ngày chạy ..........)

## A. Đăng nhập và phân quyền
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| A1 | Đăng nhập `admin/admin123` | Vào màn hình chính, sidebar có đủ 9 mục (gồm Nhân viên, Quản trị) | | |
| A2 | Đăng nhập `banhang/123456` | Sidebar chỉ có Trang chủ, Bán hàng, Khách hàng, Báo cáo & Thống kê | | |
| A3 | Đăng nhập `thukho/123456` | Sidebar chỉ có Trang chủ, Sách, Danh mục, Nhập kho | | |
| A4 | Nhập sai mật khẩu 3 lần liên tiếp | Thông báo khóa 30 giây, nút đăng nhập không cho vào trong thời gian đó | | |
| A5 | Ô đăng nhập khi mới mở | Trống (không điền sẵn tài khoản/mật khẩu) | | |
| A6 | Đăng xuất rồi đăng nhập tài khoản khác | Quay về màn hình đăng nhập; menu đổi theo vai trò mới | | |

## B. Bán hàng (POS)
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| B1 | Mở POS | Giỏ hàng **rỗng**, khách mặc định là Khách lẻ, SL mặc định 1 | | |
| B2 | Thanh toán khi giỏ rỗng | Thông báo "Giỏ hàng đang trống", không tạo hóa đơn | | |
| B3 | Thêm 1 sách với SL lớn hơn tồn | Thông báo "chỉ còn N cuốn, không đủ...", không thêm vào giỏ | | |
| B4 | Thêm 2 lần cùng sách sao cho tổng vượt tồn | Lần thứ hai bị chặn (tính cả số lượng đã có trong giỏ) | | |
| B5 | Khách lẻ, đơn từ 500.000 đ | Giảm giá 0, nhãn ghi "khách lẻ không được giảm" | | |
| B6 | Khách hạng Vàng, đơn ≥ 500.000 đ | Giảm 7%, làm tròn nghìn đồng; nhãn ghi hạng và tỷ lệ | | |
| B7 | Khách thành viên, đơn < 500.000 đ | Giảm 0, nhãn ghi "đơn từ 500.000 đ mới được giảm" | | |
| B8 | Xem trước hóa đơn | Hộp thoại hiện nội dung hóa đơn dạng văn bản | | |
| B9 | Thanh toán thành công | Tồn kho giảm đúng số lượng; điểm khách tăng; hóa đơn xuất hiện ở tab Lịch sử | | |
| B10 | Tab Lịch sử: chọn hóa đơn | Lưới dưới hiện đúng các dòng chi tiết của hóa đơn đó | | |
| B11 | Đăng nhập `thukho`, thử mở POS | Không có mục Bán hàng | | |

## C. Nhập kho
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| C1 | Chọn sách | Đơn giá nhập tự đề xuất theo giá nhập hiện tại | | |
| C2 | Lưu phiếu nhập hợp lệ | Thông báo thành công; tồn kho tăng đúng số lượng | | |
| C3 | Lưu khi SL = 0 hoặc chưa chọn NCC | Thông báo lỗi nghiệp vụ, không ghi dữ liệu | | |
| C4 | Chọn phiếu nhập ở lưới trên | Lưới dưới hiện chi tiết phiếu đó | | |

## D. Danh mục và CRUD qua Stored Procedure
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| D1 | Thêm thể loại mới (mã chưa có) | Thêm thành công, lưới cập nhật | | |
| D2 | Thêm thể loại trùng mã | Thông báo "Mã thể loại đã tồn tại." | | |
| D3 | Sửa tên tác giả, NXB, NCC | Cập nhật thành công | | |
| D4 | Xóa thể loại đang có sách | Thông báo "đang có sách nên không thể xóa" | | |
| D5 | Xóa tác giả / NXB chưa dùng | Xóa thành công | | |
| D6 | Xóa NCC đã có phiếu nhập | Thông báo không thể xóa | | |
| D7 | Thêm khách hàng trùng số điện thoại | Thông báo SĐT đã đăng ký | | |
| D8 | Xóa khách hàng đã có hóa đơn | Thông báo không thể xóa | | |

## E. Quản trị, log, xử lý lỗi
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| E1 | `admin` mở Quản trị → Nhật ký | Thấy các thao tác vừa làm (đăng nhập, thanh toán, nhập kho...) kèm người dùng, thời gian | | |
| E2 | Tab Hiệu năng nghiệp vụ | Có số lần, thời gian TB/nhanh nhất/chậm nhất cho DANG_NHAP, THANH_TOAN, NHAP_KHO | | |
| E3 | Đăng nhập sai | Có dòng DANG_NHAP "Thất bại" trong nhật ký | | |
| E4 | Tắt dịch vụ SQL Server rồi đăng nhập | Thông báo "Không kết nối được tới SQL Server...", không hiện chuỗi kết nối/stack trace; file log có chi tiết | | |
| E5 | Mở thư mục `Logs` | Có file `yyyyMMdd.log` | | |

## F. Báo cáo
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| F1 | Thống kê → Xuất Excel | Tạo được file .xlsx đúng dữ liệu đang hiển thị | | |
| F2 | Thanh toán xong → chọn In hóa đơn | Cửa sổ Crystal Report hiện hóa đơn đúng mã, đúng tiền | | |
| F3 | Thống kê (mục 1 hoặc 3) → Crystal Report | Hiện báo cáo Sách bán chạy / Doanh thu theo tháng | | |
| F4 | Trong cửa sổ báo cáo: Xuất PDF, Xuất Excel | Tạo được file mở được | | |
| F5 | Chưa build project Reports | Ứng dụng chính báo thông báo hướng dẫn, không văng lỗi | | |

## G. Nhập liệu, tìm kiếm nâng cao, phân trang (bổ sung v2.1)
| TC | Thao tác | Kết quả mong đợi | Kết quả thực tế | Đạt/Không |
|---|---|---|---|---|
| G1 | Quản lý Khách hàng: để trống Họ tên rồi bấm Lưu | Biểu tượng ErrorProvider đỏ hiện cạnh ô Họ tên, không lưu | | |
| G2 | Gõ chữ cái vào ô Điện thoại | Ký tự chữ bị chặn, chỉ nhận chữ số (tối đa 11 số) | | |
| G3 | Nhập SĐT "123" rồi Lưu | ErrorProvider báo SĐT không hợp lệ | | |
| G4 | Sửa một ô trong form Khách hàng/Sách/Nhân viên rồi bấm nút X đóng form | Hỏi "Có thay đổi chưa lưu...", chọn No thì form không đóng | | |
| G5 | Chỉ chọn dòng trên lưới (không sửa gì) rồi đóng form | Đóng luôn, không hỏi | | |
| G6 | POS: thêm sách vào giỏ rồi đóng form | Hỏi xác nhận bỏ giỏ hàng | | |
| G7 | Màn hình chính: bấm X thoát | Hỏi "Bạn có chắc muốn thoát"; menu Đăng xuất thì không hỏi | | |
| G8 | Quản lý Sách: mở form | Lưới chỉ có 10 dòng, hiện "Trang 1 / 3", nút Trước bị khóa | | |
| G9 | Bấm Sau, Sau, Trước | Chuyển trang đúng; trang cuối khóa nút Sau | | |
| G10 | Nhập khoảng giá bán (ví dụ 100.000 - 200.000) rồi Tìm | Chỉ còn sách trong khoảng giá; về trang 1; tổng số cập nhật | | |
| G11 | Nhập Giá từ lớn hơn Giá đến | ErrorProvider báo lỗi cạnh ô "đến", không truy vấn | | |
| G12 | Chọn Sắp xếp = Giá bán, bật Giảm dần | Lưới sắp xếp giá giảm dần trên toàn bộ kết quả (không chỉ trang hiện tại) | | |
| G13 | Gõ từ khóa rồi nhấn Enter | Tự động tìm kiếm | | |
| G14 | Bấm "Xuất Excel" khi đang lọc | File Excel chứa toàn bộ kết quả khớp bộ lọc (không chỉ 10 dòng) | | |
| G15 | Quản lý Nhân viên: bấm "+ Thêm NV" | Mã nhân viên tự sinh tiếp theo (NV06...), ô mã không sửa tay được | | |
