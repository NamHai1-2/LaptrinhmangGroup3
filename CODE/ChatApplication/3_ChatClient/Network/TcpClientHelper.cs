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
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listenTask;
        private readonly object _writeLock = new object();

        public bool IsConnected
        {
            get
            {
                try
                {
                    return _tcpClient != null && _tcpClient.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string ServerIp { get; private set; }
        public int ServerPort { get; private set; }

        public event Action<MessagePacket> OnMessageReceived;
        public event Action<string> OnRawDataReceived;
        public event Action<string> OnStatusChanged;
        public event Action<string> OnError;

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            try
            {
                if (IsConnected)
                {
                    OnStatusChanged?.Invoke("Client đã kết nối sẵn.");
                    return true;
                }

                ServerIp = ip;
                ServerPort = port;

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);

                _networkStream = _tcpClient.GetStream();
                _reader = new StreamReader(_networkStream);
                _writer = new StreamWriter(_networkStream)
                {
                    AutoFlush = true
                };

                _cancellationTokenSource = new CancellationTokenSource();

                OnStatusChanged?.Invoke($"Đã kết nối đến server {ip}:{port}");

                _listenTask = Task.Run(() => ListenForMessagesAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Kết nối thất bại: " + ex.Message);
                Cleanup();
                return false;
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    string rawData = await _reader.ReadLineAsync();

                    if (rawData == null)
                    {
                        OnStatusChanged?.Invoke("Server đã ngắt kết nối.");
                        Disconnect(false);
                        break;
                    }

                    OnRawDataReceived?.Invoke(rawData);

                    try
                    {
                        MessagePacket packet = JsonParser.Deserialize<MessagePacket>(rawData);

                        if (packet != null)
                        {
                            OnMessageReceived?.Invoke(packet);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke("Lỗi parse dữ liệu từ server: " + ex.Message);
                    }
                }
            }
            catch (IOException)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    OnError?.Invoke("Mất kết nối tới server.");
                    Disconnect(false);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    OnError?.Invoke("Lỗi khi nhận dữ liệu: " + ex.Message);
                    Disconnect(false);
                }
            }
        }

        public async Task<bool> SendMessageAsync(MessagePacket packet)
        {
            try
            {
                if (!IsConnected)
                {
                    OnError?.Invoke("Chưa kết nối server.");
                    return false;
                }

                if (packet == null)
                {
                    OnError?.Invoke("MessagePacket không hợp lệ.");
                    return false;
                }

                string json = JsonParser.Serialize(packet);
                return await SendRawAsync(json);
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Lỗi gửi tin nhắn: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> SendRawAsync(string rawData)
        {
            try
            {
                if (!IsConnected)
                {
                    OnError?.Invoke("Chưa kết nối server.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(rawData))
                {
                    OnError?.Invoke("Dữ liệu gửi rỗng.");
                    return false;
                }

                lock (_writeLock)
                {
                    _writer.WriteLine(rawData);
                }

                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Lỗi gửi dữ liệu: " + ex.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            Disconnect(true);
        }

        private void Disconnect(bool raiseStatusEvent)
        {
            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                Cleanup();

                if (raiseStatusEvent)
                {
                    OnStatusChanged?.Invoke("Đã ngắt kết nối khỏi server.");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Lỗi ngắt kết nối: " + ex.Message);
            }
        }

        private void Cleanup()
        {
            try { _reader?.Close(); } catch { }
            try { _writer?.Close(); } catch { }
            try { _networkStream?.Close(); } catch { }
            try { _tcpClient?.Close(); } catch { }

            try { _reader?.Dispose(); } catch { }
            try { _writer?.Dispose(); } catch { }
            try { _networkStream?.Dispose(); } catch { }
            try { _tcpClient?.Dispose(); } catch { }
            try { _cancellationTokenSource?.Dispose(); } catch { }

            _reader = null;
            _writer = null;
            _networkStream = null;
            _tcpClient = null;
            _cancellationTokenSource = null;
            _listenTask = null;
        }
    }
}
