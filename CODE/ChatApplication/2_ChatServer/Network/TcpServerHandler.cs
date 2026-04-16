using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;
using _2_ChatServer.Data;

namespace _2_ChatServer.Network
{
    public class TcpServerHandler
    {
        private TcpListener _server;
        private Dictionary<string, ClientConnection> _onlineUsers = new Dictionary<string, ClientConnection>();
        private DatabaseHelper _db = new DatabaseHelper();
        public Action<string> OnLogEvent;

        public async Task StartServerAsync(int port)
        {
            try
            {
                _db.Initialize();
                _server = new TcpListener(IPAddress.Any, port);
                _server.Start();
                OnLogEvent?.Invoke($"[HỆ THỐNG] Server khởi động tại Port {port}");

                while (true)
                {
                    TcpClient tcpClient = await _server.AcceptTcpClientAsync();
                    ClientConnection client = new ClientConnection(tcpClient);
                    OnLogEvent?.Invoke($"[MẠNG] Thiết bị kết nối: {client.ClientEndPoint}");

                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("aborted") && !ex.Message.Contains("WSACancelBlockingCall"))
                {
                    OnLogEvent?.Invoke($"[LỖI SERVER] {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(ClientConnection client)
        {
            try
            {
                while (client.IsConnected)
                {
                    MessagePacket packet = client.ReceiveMessage();
                    if (packet == null) break;

                    if (packet.Command == CommandType.Login)
                    {
                        if (_db.CheckLogin(packet.Sender, packet.Content))
                        {
                            client.Username = packet.Sender;
                            lock (_onlineUsers) { _onlineUsers[client.Username] = client; }

                            client.SendMessage(new MessagePacket { Command = CommandType.LoginSuccess });
                            OnLogEvent?.Invoke($"[ĐĂNG NHẬP] {client.Username} đã vào phòng.");
                            BroadcastUserList();
                        }
                        else
                        {
                            client.SendMessage(new MessagePacket { Command = CommandType.LoginFail });
                        }
                    }
                    else if (packet.Command == CommandType.Register)
                    {
                        if (_db.RegisterUser(packet.Sender, packet.Content))
                        {
                            client.SendMessage(new MessagePacket { Command = CommandType.RegisterSuccess });
                            OnLogEvent?.Invoke($"[ĐĂNG KÝ] Tài khoản mới: {packet.Sender}");
                        }
                        else
                        {
                            client.SendMessage(new MessagePacket { Command = CommandType.RegisterFail });
                            OnLogEvent?.Invoke($"[ĐĂNG KÝ LỖI] Trùng tên tài khoản: {packet.Sender}");
                        }
                    }
                    else if (packet.Command == CommandType.BroadcastMessage)
                    {
                        OnLogEvent?.Invoke($"[CHAT TỔNG] {packet.Sender}: {packet.Content}");
                        Broadcast(packet);
                    }
                    else if (packet.Command == CommandType.PrivateMessage)
                    {
                        packet.Sender = client.Username; 
                        OnLogEvent?.Invoke($"[CHAT RIÊNG] {packet.Sender} -> {packet.Receiver}: {packet.Content}");
                        SendPrivate(packet.Receiver, packet);
                    }
                    else if (packet.Command == CommandType.Disconnect)
                    {
                        break;
                    }
                }
            }
            catch { }
            finally
            {
                if (!string.IsNullOrEmpty(client.Username))
                {
                    lock (_onlineUsers) { _onlineUsers.Remove(client.Username); }
                    OnLogEvent?.Invoke($"[THOÁT] {client.Username} đã rời phòng.");
                    BroadcastUserList();
                }
                client.Close();
            }
        }

        private void Broadcast(MessagePacket packet)
        {
            lock (_onlineUsers)
            {
                foreach (var user in _onlineUsers.Values)
                {
                    user.SendMessage(packet);
                }
            }
        }

        private void BroadcastUserList()
        {
            lock (_onlineUsers)
            {
                string usersString = string.Join(",", _onlineUsers.Keys);
                Broadcast(new MessagePacket { Command = CommandType.UserListUpdate, Content = usersString });
            }
        }
        private void SendPrivate(string receiver, MessagePacket packet)
        {
            lock (_onlineUsers)
            {
                if (_onlineUsers.ContainsKey(receiver))
                {
                    _onlineUsers[receiver].SendMessage(packet);
                }
            }
        }    
        public void StopServer()
        {
            try
            {
                
                _server?.Stop();

                lock (_onlineUsers)
                {
                    foreach (var client in _onlineUsers.Values)
                    {
                        client.Close();
                    }
                    _onlineUsers.Clear();
                }
            }
            catch { }
        }
    }
}