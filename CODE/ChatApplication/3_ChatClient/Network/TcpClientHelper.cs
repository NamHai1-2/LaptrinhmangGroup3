using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;

namespace _3_ChatClient.Network
{
    public class TcpClientHelper
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cts;
        

        public string CurrentUsername { get; set; }
        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        public event Action ServerDisconnected;
        public event Action<MessagePacket> OnMessageReceived;
        public event Action<string> OnStatusChanged;

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            try
            {
                if (IsConnected) return true;

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);

                _networkStream = _tcpClient.GetStream();
                _reader = new StreamReader(_networkStream);
                _writer = new StreamWriter(_networkStream) { AutoFlush = true };

                _cts = new CancellationTokenSource();
                OnStatusChanged?.Invoke($"Đã kết nối {ip}:{port}");

                _ = Task.Run(() => ListenForMessagesAsync(_cts.Token));
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("Kết nối thất bại: " + ex.Message);
                return false;
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && IsConnected)
                {
                    string rawData = await _reader.ReadLineAsync();
                    
                    if (rawData == null)
                    {
                        ServerDisconnected?.Invoke(); 
                        break; 
                    }

                    var packet = JsonParser.Deserialize<MessagePacket>(rawData);
                    if (packet != null) OnMessageReceived?.Invoke(packet);

                }          

                    
            }
            catch
            {
                ServerDisconnected?.Invoke();
            }
            finally { Disconnect(); }
        }

        public async Task<bool> SendMessageAsync(MessagePacket packet)
        {
            try
            {
                if (!IsConnected || packet == null) return false;
                string json = JsonParser.Serialize(packet);
                await _writer.WriteLineAsync(json);
                return true;
            }
            catch { return false; }
        }

        public void Disconnect()
        {
            try
            {
                _cts?.Cancel();
                _reader?.Close();
                _writer?.Close();
                _networkStream?.Close();
                _tcpClient?.Close();
                OnStatusChanged?.Invoke("Đã ngắt kết nối.");
            }
            catch { }
        }
    }
}