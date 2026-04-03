TCP\_Chat\_Solution/

│

├── .gitignore                   <-- Đặt ở ngay ngoài cùng (Rất quan trọng!)

├── TCP\_Chat.sln                 <-- File Solution gộp 3 project

│

├── 1\_SharedLibrary/             <-- Project 1: Dùng chung cho cả 2 bên (Class Library)

│   ├── Models/

│   │   ├── User.cs              <-- Class định nghĩa cấu trúc dữ liệu người dùng

│   │   ├── MessagePacket.cs     <-- Class định nghĩa cấu trúc gói tin (chứa Sender, Content...)

│   └── Utils/

│       ├── JsonParser.cs        <-- Các hàm parse chuỗi thành JSON (đúng như sơ đồ của bạn)

│       └── Constants.cs         <-- Lưu các hằng số: Port (vd: 8080), CommandTypes (LOGIN, CHAT)

│

├── 2\_ChatServer/                <-- Project 2: Dành cho Thành viên 1 \& 2 (Windows Forms/WPF App)

│   ├── Data/

│   │   ├── DatabaseHelper.cs    <-- Class chuyên xử lý kết nối và lệnh SQLite

│   │   └── chat\_database.db     <-- File database vật lý (sẽ được tạo tự động)

│   ├── Network/

│   │   ├── TcpServerHandler.cs  <-- Chứa class TcpListener, lắng nghe kết nối

│   │   └── ClientConnection.cs  <-- Class quản lý luồng (thread) riêng cho từng Client

│   └── UI/

│       └── ServerDashboard.cs   <-- Giao diện Server (Nút Start/Stop, Log trạng thái mạng)

│

└── 3\_ChatClient/                <-- Project 3: Dành cho Thành viên 3 \& 4 (Windows Forms/WPF App)

&#x20;   ├── Network/

&#x20;   │   └── TcpClientHelper.cs   <-- Chứa class TcpClient kết nối đến Server

&#x20;   └── UI/

&#x20;       ├── LoginForm.cs         <-- Màn hình đăng nhập

&#x20;       ├── MainChatForm.cs      <-- Màn hình chat chính (hiển thị danh sách online, khung chat)

&#x20;       └── CustomControls/      <-- (Tùy chọn) Chứa các bong bóng chat tự thiết kế cho đẹp

