using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;
using _2_ChatServer.Network;

namespace _2_ChatServer.UI
{
    public partial class ServerDashboard : Form
    {
        private TcpListener _listener;
        private bool _isRunning;
        private readonly List<ClientConnection> _clients = new List<ClientConnection>();
        private readonly object _clientLock = new object();

        public ServerDashboard()
        {
            InitializeComponent();
        }

        private void ServerDashboard_Load(object sender, EventArgs e)
        {
            txtPort.Text = Constants.DEFAULT_SERVER_PORT.ToString();
            lblStatus.Text = "Server chưa chạy";
        }

        private async void btnStartServer_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                AppendLog("Server đã chạy rồi.");
                return;
            }

            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Port không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort.Focus();
                return;
            }

            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;

                lblStatus.Text = $"Server đang chạy tại cổng {port}";
                btnStartServer.Enabled = false;
                btnStopServer.Enabled = true;

                AppendLog($"[SERVER] Đã khởi động tại cổng {port}");

                await Task.Run(AcceptClientsAsync);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi động server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("[LỖI] " + ex.Message);
            }
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                    ClientConnection client = new ClientConnection(tcpClient);

                    lock (_clientLock)
                    {
                        _clients.Add(client);
                    }

                    AppendLog($"[KẾT NỐI] Client mới: {client.ClientEndPoint}");
                    UpdateClientList();

                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        AppendLog("[LỖI ACCEPT] " + ex.Message);
                    }
                }
            }
        }

        private async Task HandleClientAsync(ClientConnection client)
        {
            try
            {
                while (_isRunning && client.IsConnected)
                {
                    MessagePacket packet = client.ReceiveMessage();

                    if (packet == null)
                    {
                        RemoveClient(client);
                        break;
                    }

                    if (packet.MessageType == Constants.MESSAGE_TYPE_LOGIN)
                    {
                        client.Username = packet.Sender;

                        AppendLog($"[LOGIN] {packet.Sender} đã đăng nhập.");
                        Broadcast(new MessagePacket
                        {
                            Sender = "SERVER",
                            Receiver = "ALL",
                            Content = $"{packet.Sender} đã vào phòng chat.",
                            Timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"),
                            MessageType = Constants.MESSAGE_TYPE_SYSTEM
                        });
                    }
                    else if (packet.MessageType == Constants.MESSAGE_TYPE_CHAT)
                    {
                        AppendLog($"[CHAT] {packet.Sender}: {packet.Content}");
                        Broadcast(packet);
                    }
                    else if (packet.MessageType == Constants.MESSAGE_TYPE_LOGOUT)
                    {
                        AppendLog($"[LOGOUT] {packet.Sender} đã đăng xuất.");
                        Broadcast(new MessagePacket
                        {
                            Sender = "SERVER",
                            Receiver = "ALL",
                            Content = $"{packet.Sender} đã rời phòng chat.",
                            Timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"),
                            MessageType = Constants.MESSAGE_TYPE_SYSTEM
                        });

                        RemoveClient(client);
                        break;
                    }
                    else
                    {
                        AppendLog($"[UNKNOWN] {packet.Sender}: {packet.Content}");
                    }

                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                AppendLog("[LỖI CLIENT] " + ex.Message);
                RemoveClient(client);
            }
        }

        private void Broadcast(MessagePacket packet)
        {
            List<ClientConnection> disconnectedClients = new List<ClientConnection>();

            lock (_clientLock)
            {
                foreach (var client in _clients)
                {
                    bool sent = client.SendMessage(packet);
                    if (!sent)
                    {
                        disconnectedClients.Add(client);
                    }
                }
            }

            foreach (var client in disconnectedClients)
            {
                RemoveClient(client);
            }
        }

        private void RemoveClient(ClientConnection client)
        {
            bool removed = false;

            lock (_clientLock)
            {
                if (_clients.Contains(client))
                {
                    _clients.Remove(client);
                    removed = true;
                }
            }

            if (removed)
            {
                string name = string.IsNullOrWhiteSpace(client.Username) ? client.ClientEndPoint : client.Username;
                AppendLog($"[NGẮT KẾT NỐI] {name}");
                client.Close();
                UpdateClientList();
            }
        }

        private void UpdateClientList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateClientList));
                return;
            }

            lstClients.Items.Clear();

            lock (_clientLock)
            {
                foreach (var client in _clients)
                {
                    string displayName = string.IsNullOrWhiteSpace(client.Username)
                        ? client.ClientEndPoint
                        : $"{client.Username} - {client.ClientEndPoint}";

                    lstClients.Items.Add(displayName);
                }
            }

            lblClientCount.Text = "Số client: " + lstClients.Items.Count;
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), message);
                return;
            }

            lstLogs.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");

            if (lstLogs.Items.Count > 0)
            {
                lstLogs.TopIndex = lstLogs.Items.Count - 1;
            }
        }

        private void StopServer()
        {
            try
            {
                _isRunning = false;

                lock (_clientLock)
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch
                        {
                        }
                    }

                    _clients.Clear();
                }

                try
                {
                    _listener?.Stop();
                }
                catch
                {
                }

                UpdateClientList();

                lblStatus.Text = "Server đã dừng";
                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;

                AppendLog("[SERVER] Đã dừng server.");
            }
            catch (Exception ex)
            {
                AppendLog("[LỖI STOP] " + ex.Message);
            }
        }

        private void ServerDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }
    }
}
