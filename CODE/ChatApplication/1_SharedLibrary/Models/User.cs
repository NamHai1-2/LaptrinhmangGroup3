using System;

namespace _1_SharedLibrary.Models
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }                 // Khóa chính trong Database
        public string Username { get; set; }        // Tên đăng nhập / Tên hiển thị
        public string Password { get; set; }        // Mật khẩu 
        public bool IsOnline { get; set; }          // Trạng thái online/offline hiện tại
        public DateTime LastLogin { get; set; }     // Thời gian đăng nhập cuối cùng

        // Hàm khởi tạo trống 
        public User()
        {
        }

        // Hàm khởi tạo có tham số để tạo User nhanh chóng
        public User(string username, string password)
        {
            Username = username;
            Password = password;
            IsOnline = false;
            LastLogin = DateTime.Now;
        }

        // Ghi đè phương thức ToString() để khi đưa vào ListBox trên UI nó tự hiện tên
        public override string ToString()
        {
            return Username + (IsOnline ? " (Online)" : " (Offline)");
        }
    }
}