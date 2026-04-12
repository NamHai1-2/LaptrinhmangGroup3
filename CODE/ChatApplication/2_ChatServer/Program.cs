using System;
using System.Windows.Forms;
using _2_ChatServer.UI;

namespace _2_ChatServer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Khởi chạy giao diện ServerDashboard đầu tiên
            Application.Run(new ServerDashboard());
        }
    }
}