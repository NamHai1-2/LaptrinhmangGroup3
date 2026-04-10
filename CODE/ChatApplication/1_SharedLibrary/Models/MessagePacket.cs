using System;

namespace _1_SharedLibrary.Models
{
    // 1. Định nghĩa các loại lệnh (Command) để Server biết Client đang muốn làm gì
    public enum CommandType
    {
        Login,              // Yêu cầu đăng nhập
        Register,           // Yêu cầu đăng ký
        BroadcastMessage,   // Gửi tin nhắn cho tất cả mọi người
        PrivateMessage,     // Gửi tin nhắn riêng cho 1 người
        UserListUpdate,     // Server gửi danh sách user online cho Client
        Disconnect          // Thông báo ngắt kết nối
    }

    // 2. Cấu trúc của Gói tin (Sẽ được biến thành chuỗi JSON để gửi đi)
    [Serializable]
    public class MessagePacket
    {
        public CommandType Command { get; set; } // Loại hành động
        public string Sender { get; set; }       // Tên người gửi
        public string Receiver { get; set; }     // Tên người nhận (để trống nếu là Broadcast)
        public string Content { get; set; }      // Nội dung tin nhắn / Hoặc mật khẩu khi đăng nhập
        public DateTime Timestamp { get; set; }  // Thời gian gửi

        // Hàm khởi tạo mặc định (Tự động gán thời gian hiện tại)
        public MessagePacket()
        {
            Timestamp = DateTime.Now;
        }
    }
}