💬 Chat Application - TCP/IP Socket Programming
📝 Giới thiệu dự án
Đây là ứng dụng trò chuyện trực tuyến (Chat Application) được xây dựng trên nền tảng C# WinForms sử dụng kiến trúc Client-Server qua giao thức TCP/IP. Dự án được thiết kế theo mô hình phân lớp nhằm tối ưu hóa việc quản lý mã nguồn và luồng dữ liệu giữa các thành viên trong nhóm.



🚀 Các tính năng chính
Hệ thống Tài khoản: Hỗ trợ Đăng ký tài khoản mới và Đăng nhập xác thực qua Database.

Chat Tổng (Broadcast): Gửi tin nhắn công khai cho tất cả mọi người đang online trong hệ thống.

Chat Riêng (Private): Chế độ nhắn tin bảo mật giữa hai cá nhân bằng cách click chuột phải vào danh sách người dùng.

Danh sách Online: Tự động cập nhật thời gian thực danh sách các thành viên đang hoạt động.

Trạng thái kết nối: Hiển thị chi tiết địa chỉ IP và Port đang kết nối ngay trên giao diện.

Quản lý Server: Giao diện Dashboard cho phép Start/Stop server và theo dõi toàn bộ nhật ký (Log) hoạt động của hệ thống.



🛠 Công nghệ sử dụng
Ngôn ngữ: C# (.NET Core/Framework).

Giao diện: Windows Forms (WinForms).

Giao thức mạng: TCP/IP Sockets (Sử dụng TcpListener và TcpClient).

Cơ sở dữ liệu: SQLite (Microsoft.Data.Sqlite) để lưu trữ thông tin người dùng và tin nhắn.

Định dạng dữ liệu: JSON (Sử dụng System.Text.Json) để đóng gói gói tin.



📂 Cấu trúc thư mục
\_1\_SharedLibrary: Chứa các Model dữ liệu (MessagePacket, User) và các tiện ích dùng chung (JsonParser, Constants).

\_2\_ChatServer: Chứa logic xử lý tại máy chủ, quản lý kết nối và tương tác Database.

\_3\_ChatClient: Chứa giao diện người dùng và logic xử lý phía máy khách.



⚙️ Hướng dẫn cài đặt và chạy

1. Yêu cầu hệ thống

&#x09;Visual Studio 2022 hoặc mới hơn.

&#x09;Cài đặt thư viện NuGet: Microsoft.Data.Sqlite cho project Server.

2. Khởi động hệ thống

&#x09;Chạy Server: Mở project 2\_ChatServer, nhấn Start Server. Mặc định Server sẽ chạy tại Port 8080.

&#x09;Chạy Client: Mở project 3\_ChatClient. Nhập chính xác IP Server (mặc định 127.0.0.1) và Port để kết nối.

3\. Sử dụng:

&#x09;Sử dụng tài khoản test: 123 / mật khẩu 123 hoặc bấm Register để tạo tài khoản mới.

&#x09;Để Chat riêng: Click chuột phải vào tên người dùng trong danh sách bên trái và chọn "Chat Riêng ".

&#x09;Để Chat tổng: Click chuột phải và chọn "Quay lại Chat Tổng".

