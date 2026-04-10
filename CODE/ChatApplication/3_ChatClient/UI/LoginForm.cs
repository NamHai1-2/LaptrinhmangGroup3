using System;
using System.Windows.Forms;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;
using _3_ChatClient.Network;

namespace _3_ChatClient.UI
{
    public partial class LoginForm : Form
    {
        private TcpClientHelper _clientHelper;

        public LoginForm()
        {
            InitializeComponent();

            _clientHelper = new TcpClientHelper();

            _clientHelper.OnStatusChanged += ClientHelper_OnStatusChanged;
            _clientHelper.OnError += ClientHelper_OnError;
            _clientHelper.OnMessageReceived += ClientHelper_OnMessageReceived;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            txtServerIp.Text = Constants.DEFAULT_SERVER_IP;
            txtPort.Text = Constants.DEFAULT_SERVER_PORT.ToString();
            lblStatus.Text = "Chưa kết nối";

            txtUsername.Focus();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string serverIp = txtServerIp.Text.Trim();
            string portText = txtPort.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(serverIp))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ IP server.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServerIp.Focus();
                return;
            }

            if (!int.TryParse(portText, out int port))
            {
                MessageBox.Show("Port không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort.Focus();
                return;
            }

            ToggleControls(false);
            lblStatus.Text = "Đang kết nối đến server...";

            bool connected = await _clientHelper.ConnectAsync(serverIp, port);

            if (!connected)
            {
                ToggleControls(true);
                return;
            }

            MessagePacket loginPacket = new MessagePacket
            {
                Sender = username,
                Receiver = "SERVER",
                Content = $"{username} đăng nhập vào hệ thống",
                Timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"),
                MessageType = Constants.MESSAGE_TYPE_LOGIN
            };

            bool sent = await _clientHelper.SendMessageAsync(loginPacket);

            if (!sent)
            {
                ToggleControls(true);
                return;
            }

            MainChatForm mainChatForm = new MainChatForm(_clientHelper, username);
            mainChatForm.FormClosed += MainChatForm_FormClosed;
            mainChatForm.Show();

            this.Hide();
        }

        private void MainChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (_clientHelper != null && _clientHelper.IsConnected)
                {
                    _clientHelper.Disconnect();
                }
            }
            catch
            {
            }

            this.Show();
            ToggleControls(true);
            lblStatus.Text = "Chưa kết nối";
        }

        private void ClientHelper_OnStatusChanged(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientHelper_OnStatusChanged(status)));
                return;
            }

            lblStatus.Text = status;
        }

        private void ClientHelper_OnError(string errorMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientHelper_OnError(errorMessage)));
                return;
            }

            lblStatus.Text = "Có lỗi xảy ra";
            MessageBox.Show(errorMessage, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ToggleControls(true);
        }

        private void ClientHelper_OnMessageReceived(MessagePacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientHelper_OnMessageReceived(packet)));
                return;
            }

            if (packet == null)
                return;

            if (packet.MessageType == Constants.MESSAGE_TYPE_SYSTEM)
            {
                lblStatus.Text = packet.Content;
            }
        }

        private void ToggleControls(bool enabled)
        {
            txtUsername.Enabled = enabled;
            txtServerIp.Enabled = enabled;
            txtPort.Enabled = enabled;
            btnLogin.Enabled = enabled;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _clientHelper?.Disconnect();
            }
            catch
            {
            }
        }
    }
}
