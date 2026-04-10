using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;
using _2_ChatServer.Data;

namespace _2_ChatServer.Network
{
    public class TcpServerHandler
    {
        private TcpListener _server;

        private Dictionary<string, TcpClient> _onlineUsers = new Dictionary<string, TcpClient>();

        private DatabaseHelper _db = new DatabaseHelper();

        public Action<string> OnLogEvent;

        public void StartServer()
        {
            try
            {
                _db.InitializeDatabase();
                _server = new TcpListener(IPAddress.Any, Constants.SERVER_PORT);
                _server.Start();
                OnLogEvent?.Invoke($"[HỆ THỐNG] Server đã khởi động tại Port {Constants.SERVER_PORT}...");

                Task.Run(() => AcceptClients());
            }
            catch (Exception ex)
            {
                OnLogEvent?.Invoke($"[LỖI HỆ THỐNG] Không thể khởi động Server: {ex.Message}");
            }
        }

        private void AcceptClients()
        {
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                client.NoDelay = true;
                client.ReceiveBufferSize = Constants.BUFFER_SIZE;
                client.SendBufferSize = Constants.BUFFER_SIZE;

                OnLogEvent?.Invoke("[MẠNG] Có một thiết bị ẩn danh vừa kết nối.");

                Task.Run(() => HandleClient(client));
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[Constants.BUFFER_SIZE];
            string currentUsername = string.Empty; 

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; 

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var packet = JsonSerializer.Deserialize<MessagePacket>(json);


                    if (packet.Command == CommandType.Login)
                    {
                        bool isOk = _db.CheckLogin(packet.Sender, packet.Content); 
                        if (isOk)
                        {
                            currentUsername = packet.Sender;

                            if (_onlineUsers.ContainsKey(currentUsername))
                                _onlineUsers[currentUsername] = client;
                            else
                                _onlineUsers.Add(currentUsername, client);

                            SendToClient(client, new MessagePacket { Command = CommandType.LoginSuccess });
                            OnLogEvent?.Invoke($"[ĐĂNG NHẬP] User '{currentUsername}' đã tham gia hệ thống.");

                            BroadcastUserList();
                        }
                        else
                        {
                            SendToClient(client, new MessagePacket { Command = CommandType.LoginFail });
                        }
                    }
                    else if (packet.Command == CommandType.BroadcastMessage)
                    {
                        OnLogEvent?.Invoke($"[CHAT TỔNG] {packet.Sender}: {packet.Content}");
                        Broadcast(packet);
                    }
                    else if (packet.Command == CommandType.PrivateMessage)
                    {
                        OnLogEvent?.Invoke($"[CHAT RIÊNG] {packet.Sender} -> {packet.Receiver}: {packet.Content}");
                        SendPrivate(packet.Receiver, packet);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (!string.IsNullOrEmpty(currentUsername) && _onlineUsers.ContainsKey(currentUsername))
                {
                    _onlineUsers.Remove(currentUsername);
                    OnLogEvent?.Invoke($"[THOÁT] User '{currentUsername}' đã rời khỏi hệ thống.");
                    BroadcastUserList(); 
                }
                client.Close();
            }
        }


        private void SendToClient(TcpClient client, MessagePacket packet)
        {
            try
            {
                string json = JsonSerializer.Serialize(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);
                client.GetStream().Write(data, 0, data.Length);
            }
            catch {  }
        }

        private void SendPrivate(string receiver, MessagePacket packet)
        {
            if (_onlineUsers.ContainsKey(receiver))
            {
                SendToClient(_onlineUsers[receiver], packet);
            }
        }

        private void Broadcast(MessagePacket packet)
        {
            
            foreach (var user in _onlineUsers)
            {
                SendToClient(user.Value, packet);
            }
        }

        private void BroadcastUserList()
        {
            string usersString = string.Join(",", _onlineUsers.Keys);

            var packet = new MessagePacket
            {
                Command = CommandType.UserListUpdate,
                Content = usersString
            };
            Broadcast(packet);
        }
    }
}