using System;
using System.Drawing;
using System.Windows.Forms;
using _1_SharedLibrary.Utils;
using _2_ChatServer.Network;

namespace _2_ChatServer.UI
{
    public partial class ServerDashboard : Form
    {
        private TcpServerHandler _serverHandler;

        
        private TextBox txtPort;
        private Label lblStatus;
        private Button btnStartServer;
        private ListBox lstLogs;

        public ServerDashboard()
        {
            
            InitializeComponentProgrammatically();

            _serverHandler = new TcpServerHandler();
            _serverHandler.OnLogEvent += AppendLog;
        }

        private Button btnStopServer; 

        private void InitializeComponentProgrammatically()
        {
            this.Text = "Server Dashboard";
            this.Size = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };

            panelTop.Controls.Add(new Label { Text = "Port:", Location = new Point(10, 15), AutoSize = true });
            txtPort = new TextBox { Location = new Point(50, 12), Width = 80, Text = "8080" };

            btnStartServer = new Button { Text = "Start Server", Location = new Point(150, 10), Width = 100, BackColor = Color.LightGreen };
            btnStartServer.Click += BtnStartServer_Click;

            // THÊM NÚT STOP SERVER
            btnStopServer = new Button { Text = "Stop Server", Location = new Point(260, 10), Width = 100, BackColor = Color.LightCoral, Enabled = false };
            btnStopServer.Click += BtnStopServer_Click;

            lblStatus = new Label { Text = "Status: Stopped", Location = new Point(380, 15), AutoSize = true, ForeColor = Color.Red };

            panelTop.Controls.Add(txtPort);
            panelTop.Controls.Add(btnStartServer);
            panelTop.Controls.Add(btnStopServer); 
            panelTop.Controls.Add(lblStatus);

            lstLogs = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };
            this.Controls.Add(lstLogs);
            this.Controls.Add(panelTop);
        }

        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Status: Stopped";
            lblStatus.ForeColor = Color.Red;
            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;
            _serverHandler.StopServer();
            AppendLog("[HỆ THỐNG] Đã ngắt máy chủ (Cần khởi động lại App để chạy phiên mới).");
        }

        private async void BtnStartServer_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Vui lòng nhập Port là một con số hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
            lblStatus.Text = "Status: Running...";
            lblStatus.ForeColor = Color.Green;
            await _serverHandler.StartServerAsync(port);
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), message);
                return;
            }

            if (message.Length > 70) 
            {
                message = message.Substring(0, 70) + "... (tin nhắn quá dài)";
            }

            lstLogs.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (lstLogs.Items.Count > 0)
                lstLogs.TopIndex = lstLogs.Items.Count - 1;
        }
    }
}