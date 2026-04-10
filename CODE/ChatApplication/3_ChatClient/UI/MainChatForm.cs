using System;
using System.Windows.Forms;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;
using _3_ChatClient.Network;

namespace _3_ChatClient.UI
{
    public partial class MainChatForm : Form
    {
        private readonly TcpClientHelper _clientHelper;
        private readonly string _currentUsername;

        public MainChatForm(TcpClientHelper clientHelper, string username)
        {
            InitializeComponent();

            _clientHelper = clientHelper;
            _currentUsername = username;

            _clientHelper.OnMessageReceived += ClientHelper_OnMessageReceived;
            _clientHelper.OnError += ClientHelper_OnError;
            _clientHelper.OnStatusChanged += ClientHelper_OnStatusChanged;
        }

        private void MainChatForm_Load(object sender, EventArgs e)
        {
            lblCurrentUser.Text = "Người dùng: " + _currentUsername;
            lblStatus.Text = _clientHelper.IsConnected ? "Đã kết nối" : "Mất kết nối";

            txtMessage.Focus();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                txtMessage.Focus();
                return;
            }

            MessagePacket packet = new MessagePacket
            {
                Sender = _currentUsername,
                Receiver = "ALL",
                Content = content,
                Timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"),
                MessageType = Constants.MESSAGE_TYPE_CHAT
            };

            bool sent = await _clientHelper.SendMessageAsync(packet);

            if (sent)
            {
                txtMessage.Clear();
                txtMessage.Focus();
            }
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            await SendLogoutPacketAsync();
            Close();
        }

        private async Task SendLogoutPacketAsync()
        {
            try
            {
                if (_clientHelper != null && _clientHelper.IsConnected)
                {
                    MessagePacket logoutPacket = new MessagePacket
                    {
                        Sender = _currentUsername,
                        Receiver = "SERVER",
                        Content = $"{_currentUsername} đã đăng xuất",
                        Timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"),
                        MessageType = Constants.MESSAGE_TYPE_LOGOUT
                    };

                    await _clientHelper.SendMessageAsync(logoutPacket);
                }
            }
            catch
            {
            }
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

            if (packet.MessageType == Constants.MESSAGE_TYPE_CHAT)
            {
                lstMessages.Items.Add($"[{packet.Timestamp}] {packet.Sender}: {packet.Content}");
            }
            else if (packet.MessageType == Constants.MESSAGE_TYPE_SYSTEM)
            {
                lstMessages.Items.Add($"[HỆ THỐNG] {packet.Content}");
            }
            else if (packet.MessageType == Constants.MESSAGE_TYPE_LOGIN)
            {
                lstMessages.Items.Add($"[ĐĂNG NHẬP] {packet.Sender} đã vào chat");
            }
            else if (packet.MessageType == Constants.MESSAGE_TYPE_LOGOUT)
            {
                lstMessages.Items.Add($"[ĐĂNG XUẤT] {packet.Sender} đã rời chat");
            }

            lstMessages.TopIndex = lstMessages.Items.Count - 1;
        }

        private void ClientHelper_OnError(string errorMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientHelper_OnError(errorMessage)));
                return;
            }

            lblStatus.Text = "Có lỗi";
            MessageBox.Show(errorMessage, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async void MainChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _clientHelper.OnMessageReceived -= ClientHelper_OnMessageReceived;
            _clientHelper.OnError -= ClientHelper_OnError;
            _clientHelper.OnStatusChanged -= ClientHelper_OnStatusChanged;

            await SendLogoutPacketAsync();

            try
            {
                _clientHelper?.Disconnect();
            }
            catch
            {
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend.PerformClick();
                e.SuppressKeyPress = true;
            }
        }
    }
}
