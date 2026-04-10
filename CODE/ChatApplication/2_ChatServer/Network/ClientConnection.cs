using System;
using System.IO;
using System.Net.Sockets;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;

namespace _2_ChatServer.Network
{
    public class ClientConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public string Username { get; set; }
        public string ClientEndPoint { get; private set; }

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

        public TcpClient TcpClient => _tcpClient;

        public ClientConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream)
            {
                AutoFlush = true
            };

            ClientEndPoint = _tcpClient.Client.RemoteEndPoint?.ToString();
            Username = string.Empty;
        }

        public string ReceiveRaw()
        {
            try
            {
                return _reader.ReadLine();
            }
            catch
            {
                return null;
            }
        }

        public MessagePacket ReceiveMessage()
        {
            try
            {
                string rawData = ReceiveRaw();

                if (string.IsNullOrWhiteSpace(rawData))
                    return null;

                return JsonParser.Deserialize<MessagePacket>(rawData);
            }
            catch
            {
                return null;
            }
        }

        public bool SendRaw(string rawData)
        {
            try
            {
                if (!IsConnected || string.IsNullOrWhiteSpace(rawData))
                    return false;

                _writer.WriteLine(rawData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendMessage(MessagePacket packet)
        {
            try
            {
                if (packet == null)
                    return false;

                string json = JsonParser.Serialize(packet);
                return SendRaw(json);
            }
            catch
            {
                return false;
            }
        }

        public void Close()
        {
            try { _reader?.Close(); } catch { }
            try { _writer?.Close(); } catch { }
            try { _networkStream?.Close(); } catch { }
            try { _tcpClient?.Close(); } catch { }

            try { _reader?.Dispose(); } catch { }
            try { _writer?.Dispose(); } catch { }
            try { _networkStream?.Dispose(); } catch { }
            try { _tcpClient?.Dispose(); } catch { }
        }
    }
}
