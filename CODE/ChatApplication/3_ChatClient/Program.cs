using System;
using System.Windows.Forms;
using _3_ChatClient.UI;

namespace _3_ChatClient
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}